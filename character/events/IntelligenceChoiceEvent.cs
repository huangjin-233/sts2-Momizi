#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Momizi.character.relics;
using Momizi.character.rewards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.events;

/// <summary>
/// 情报抉择事件：
/// - 问号节点：花 3 情报（Tengu.IntelligenceCost），从本幕两个候选事件中二选一（可放弃，直接进入原事件）；
/// - 小怪节点：普通遭遇 vs 强化遭遇（随机一个词条，词条与奖励可见）。
/// 由白狼天狗遗物在进入节点前通过 ModifyNextEvent / CreateRoom 补丁配置本事件。
/// </summary>
[RegisterSharedEvent]
public class IntelligenceChoiceEvent : ModEventTemplate
{
    /// <summary>小怪节点强化词条（按档划分，词条奖励各自独立随机抽 1）。</summary>
    public enum MonsterModifier
    {
        None = 0,
        // 一档
        /// <summary>全敌开局 18 格挡</summary>
        T1Block18 = 1,
        /// <summary>回合开始对全体友军 2 层脆弱</summary>
        T1AllyVulnerable2 = 2,
        /// <summary>全敌 4 层活力</summary>
        T1Vigor4 = 3,
        /// <summary>全敌 HP +18</summary>
        T1Hp18 = 4,
        /// <summary>全敌 4 层再生</summary>
        T1Regen4 = 5,
        // 二档
        /// <summary>全敌 6 层覆甲</summary>
        T2Plating6 = 6,
        /// <summary>全敌开局 12 格挡 + 3 层覆甲</summary>
        T2Block12Plating3 = 7,
        /// <summary>全敌力量 +2</summary>
        T2Strength2 = 8,
        /// <summary>每死一个敌人，其余 +3 力量</summary>
        T2DeathStrength3 = 9,
        /// <summary>全敌 8 层再生</summary>
        T2Regen8 = 10,
        /// <summary>回合开始对全体友军 2 层虚弱</summary>
        T2AllyWeak2 = 11,
        /// <summary>全敌 2 层荆棘</summary>
        T2Thorns2 = 12,
        /// <summary>全敌 2 层人工制品</summary>
        T2Artifact2 = 13,
        /// <summary>全敌 HP +26</summary>
        T2Hp26 = 14,
        // 三档
        /// <summary>全敌 3 层滑溜</summary>
        T3Slippery3 = 15,
        /// <summary>全敌力量 +3 且 6 层覆甲</summary>
        T3Str3Plating6 = 16,
        /// <summary>额外增加 1 只同类型敌人</summary>
        T3ExtraEnemy1 = 17,
        /// <summary>全敌 HP +56</summary>
        T3Hp56 = 18,
    }

    /// <summary>小怪节点强化遭遇的奖励抽选（每档基础奖励固定，抽选池各自独立随机抽 1）。</summary>
    public enum BoostedRewardOption
    {
        None = 0,
        // 一档抽选池
        Potion1 = 1,          // 药水 ×1
        Heal10 = 2,           // 回复 10 点生命
        UpgradeRandom1 = 3,   // 升级 1 张随机卡牌（一/二档共用）
        RetainRandom = 4,     // 给 1 张卡附「保留」
        GetCharCard = 5,      // 获得 1 张随机本角色卡
        // 二档抽选池
        RemoveCard = 6,       // 移除 1 张卡（自选，仅第 2 幕之后；二/三档共用）
        TransformCard = 7,    // 转化 1 张卡（自选）
        Potion2 = 8,          // 药水 ×2
        MaxHp5 = 9,           // 最大生命 +5
        // 三档抽选池
        RandomRelic = 10,     // 随机普通遗物
        EnchantCard = 11,     // 附魔 1 张卡（自选）
        UpgradeChoose1 = 12,  // 升级 1 张自选卡
        UpgradeRandom2 = 13,  // 升级 2 张随机卡
    }

    // ---- 进入节点前由遗物配置 ----
    public EventModel? CandidateA;
    public EventModel? CandidateB;
    public EventModel? FallbackEvent;        // 问号节点：放弃时进入的原事件
    public EncounterModel? NormalEncounter;  // 小怪节点：普通遭遇
    public int Tier;                         // 小怪节点：奖励档位 1..3
    public MonsterModifier Modifier;         // 小怪节点：强化词条（从档词条池随机抽 1）
    public BoostedRewardOption RewardPick;   // 小怪节点：奖励抽选（从档抽选池随机抽 1）
    public Tengu? DesignatedTengu;           // 小怪节点：多人时“领头”玩家的遗物（一人选全队享）
    public bool IsConfigured;

    // 预览时生成的可变遭遇副本（规范模型只读，生成会触发 AssertMutable），战斗复用保证“预览即战果”
    private EncounterModel? _generatedEncounter;

    // 防止同一抉择事件被多次触发时重复扣情报（跨客户端/回放只扣一次）
    private bool _intelligenceSpent;

    // 本事件需要切换到战斗房间，必须是共享事件（单人无差别）
    public override bool IsShared => true;

    // 事件内可随机使用的图：每次进入事件时从中随机选一张固定显示（不选占位图 none）
    private static readonly string[] EventImages =
    [
        "res://images/events/choice_one.jpg",
        "res://images/events/choice_two.jpg",
    ];

    private string? _portrait;

    // 事件图片可复用：不加去重限制。每次进入事件都从池中重新随机选一张，允许后续重复选中同一张图。
    // 是否有真实的候选事件/遭遇（用于决定是否显示随机图而非占位图 none）
    private bool HasConfiguredContent => (CandidateA != null && CandidateB != null) || NormalEncounter != null;

    // 每次进入事件时重新随机选图（未配置走占位图 none）。
    // 注意：AssetProfile 可能先于 OnRoomEnter 被读取，因此这里执行主选、
    // AssetProfile 里的兜底逻辑确保即使先被读取也不会显示成 none。
    public override void OnRoomEnter()
    {
        _portrait = HasConfiguredContent ? EventImages[Random.Shared.Next(EventImages.Length)] : null;
    }

    public override EventAssetProfile AssetProfile
    {
        get
        {
            // 兜底：若 AssetProfile 先于 OnRoomEnter 被读取（_portrait 仍为 null），立即随机一张
            if (HasConfiguredContent && _portrait == null)
            {
                _portrait = EventImages[Random.Shared.Next(EventImages.Length)];
            }
            return new EventAssetProfile(InitialPortraitPath: HasConfiguredContent ? _portrait! : "res://images/events/none.png");
        }
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        // 防御：万一被随机事件池抽到未配置的实例，直接离开
        if (!IsConfigured)
        {
            return [new EventOption(this, Finish, OptionKey("leave"))];
        }

        // 问号节点：保留“原事件” vs 花 1 情报换成“新事件”。
        // 选项标题用 events.json 的 .title 覆盖为“原事件/新事件”，描述只显示事件名。
        if (CandidateA != null && FallbackEvent != null)
        {
            return
            [
                new EventOption(this, Leave, L10NLookup(OptionKey("original") + ".title"), FallbackEvent.Title,
                    OptionKey("original"), Array.Empty<IHoverTip>()),
                new EventOption(this, () => ChooseCandidate(CandidateA), L10NLookup(OptionKey("candidate_a") + ".title"), CandidateA.Title,
                    OptionKey("candidate_a"), Array.Empty<IHoverTip>()),
            ];
        }

        // 小怪节点：普通 vs 强化（强化词条随节点随机）。
        // 两个选项都显示将遭遇的怪物（生成一次，预览即战果）。
        if (NormalEncounter != null)
        {
            return
            [
                new EventOption(this, ChooseNormalCombat,
                    L10NLookup(OptionKey("normal") + ".title"),
                    MonsterPreviewDescription(OptionKey("normal") + ".description"),
                    OptionKey("normal"), Array.Empty<IHoverTip>()),
                new EventOption(this, ChooseBoostedCombat,
                    L10NLookup(OptionKey("boosted") + ".title"),
                    BoostedPreviewDescription(),
                    OptionKey("boosted"), Array.Empty<IHoverTip>()),
            ];
        }

        return [new EventOption(this, Finish, OptionKey("leave"))];
    }

    // ---- 问号节点 ----

    // 选中候选事件：扣 1 情报，切入该事件（只扣一次，防重复）
    private Task ChooseCandidate(EventModel target)
    {
        Tengu? tengu = DesignatedTengu ?? Owner?.GetRelic<Tengu>();
        if (!_intelligenceSpent)
        {
            _intelligenceSpent = true;
            tengu?.SpendIntelligence(Tengu.IntelligenceCost);
        }
        if (target != null && Owner != null && Owner.RunState is RunState runState)
        {
            runState.AddVisitedEvent(target);
        }
        return EnterEvent(target);
    }

    // 放弃：不扣情报，直接进入原事件
    private Task Leave() => EnterEvent(FallbackEvent);

    private Task EnterEvent(EventModel? target)
    {
        if (target == null)
        {
            return Finish();
        }
        // 把目标事件房间压在抉择房间之上；目标事件结束后回到地图时一起清理
        return RunManager.Instance.EnterRoomWithoutExitingCurrentRoom(new EventRoom(target), true);
    }

    // ---- 小怪节点 ----

    private Task ChooseNormalCombat()
    {
        BoostedRewardManager.ClearPending();
        return EnterChosenCombat();
    }

    // 强化遭遇：词条写入 Tengu 的 static 待生效字段（共享事件动作会在所有客户端/玩家复制执行，
    // static 保证全体一致），使强化对所有玩家生效；扣情报只发生在“领头”玩家（一人选全队享）。
    private Task ChooseBoostedCombat()
    {
        // 放在扣费守卫外：确保每个复制执行的客户端都写入同一 static 值（档位 + 词条 + 奖励抽选）
        BoostedRewardManager.SetPending(Tier, Modifier, RewardPick);
        Tengu? tengu = DesignatedTengu ?? Owner?.GetRelic<Tengu>();
        if (!_intelligenceSpent && tengu != null)
        {
            _intelligenceSpent = true;
            tengu.SpendIntelligence(Tengu.IntelligenceCost);
        }
        return EnterChosenCombat();
    }

    private Task EnterChosenCombat()
    {
        // 本节点被拦截成事件后，战斗房间压栈不经过普通房间入口，
        // 若不手动推进遭遇游标，下一场战斗会重复出现相同怪物。
        if (Owner?.RunState is RunState runState)
        {
            runState.Act.MarkRoomVisited(RoomType.Monster);
        }
        // 复用预览时生成的可变遭遇副本，保证预览即战果；未预览过则新建可变副本
        EnterCombatWithoutExitingEvent(_generatedEncounter ?? NormalEncounter!.ToMutable(), Array.Empty<Reward>(), shouldResumeAfterCombat: false);
        return Task.CompletedTask;
    }

    // ---- 工具 ----

    // 生成将遭遇的怪物，把怪物名拼进选项描述（fallbackKey 即该选项的基础描述，含词条与奖励文本）；
    // 战斗使用同一 EncounterModel，故预览即战果。失败时回退到原本的选项描述。
    private LocString MonsterPreviewDescription(string fallbackKey)
    {
        var loc = L10NLookup(fallbackKey);
        AppendMonsterNames(loc);
        return loc;
    }

    // 强化遭遇选项描述：基础模板 boosted.description，拼入 {Words}（词条中文）、{Reward}（奖励中文）、{Monsters}。
    private LocString BoostedPreviewDescription()
    {
        var loc = L10NLookup(OptionKey("boosted") + ".description");
        loc.Add("Words", L10NLookup(OptionKey("boosted_words") + "." + WordsKey(Modifier)).GetFormattedText());
        loc.Add("BaseReward", L10NLookup(OptionKey("boosted_base") + "." + Tier).GetFormattedText());
        loc.Add("Reward", L10NLookup(OptionKey("boosted_reward") + "." + RewardKey(RewardPick)).GetFormattedText());
        AppendMonsterNames(loc);
        return loc;
    }

    // 在可变遭遇副本上生成怪物（预览即战果），并把 {Monsters} 占位替换为怪物名；
    // 三档「额外 1 只同类敌人」词条在此处追加怪物槽，保证预览与战斗一致。
    private void AppendMonsterNames(LocString loc)
    {
        var state = Owner?.RunState;
        if (NormalEncounter != null && state != null)
        {
            _generatedEncounter ??= NormalEncounter.ToMutable();
            if (!_generatedEncounter.HaveMonstersBeenGenerated)
            {
                _generatedEncounter.GenerateMonstersWithSlots(state);
                if (Modifier == MonsterModifier.T3ExtraEnemy1 && _generatedEncounter.MonstersWithSlots is List<(Creature, int)> slots && slots.Count > 0)
                {
                    // 复用第一只怪的结构追加一个槽位（同实例；若引擎去重则退化为不生效，游戏内验证）
                    slots.Add(slots[0]);
                }
            }
            var names = _generatedEncounter.MonstersWithSlots
                .Select(m => m.Item1.Title?.GetFormattedText())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            if (names.Count > 0)
            {
                loc.Add("Monsters", string.Join("、", names));
            }
        }
    }

    private string OptionKey(string suffix) => Id.Entry + ".pages.INITIAL.options." + suffix;

    // 词条本地化键后缀
    private string WordsKey(MonsterModifier mod) => mod switch
    {
        MonsterModifier.T1Block18 => "t1_block18",
        MonsterModifier.T1AllyVulnerable2 => "t1_ally_vulnerable2",
        MonsterModifier.T1Vigor4 => "t1_vigor4",
        MonsterModifier.T1Hp18 => "t1_hp18",
        MonsterModifier.T1Regen4 => "t1_regen4",
        MonsterModifier.T2Plating6 => "t2_plating6",
        MonsterModifier.T2Block12Plating3 => "t2_block12_plating3",
        MonsterModifier.T2Strength2 => "t2_strength2",
        MonsterModifier.T2DeathStrength3 => "t2_death_strength3",
        MonsterModifier.T2Regen8 => "t2_regen8",
        MonsterModifier.T2AllyWeak2 => "t2_ally_weak2",
        MonsterModifier.T2Thorns2 => "t2_thorns2",
        MonsterModifier.T2Artifact2 => "t2_artifact2",
        MonsterModifier.T2Hp26 => "t2_hp26",
        MonsterModifier.T3Slippery3 => "t3_slippery3",
        MonsterModifier.T3Str3Plating6 => "t3_str3_plating6",
        MonsterModifier.T3ExtraEnemy1 => "t3_extra_enemy1",
        MonsterModifier.T3Hp56 => "t3_hp56",
        _ => "none",
    };

    // 奖励抽选本地化键后缀
    private string RewardKey(BoostedRewardOption opt) => opt switch
    {
        BoostedRewardOption.Potion1 => "potion1",
        BoostedRewardOption.Heal10 => "heal10",
        BoostedRewardOption.UpgradeRandom1 => "upgrade_random1",
        BoostedRewardOption.RetainRandom => "retain_random",
        BoostedRewardOption.GetCharCard => "get_char_card",
        BoostedRewardOption.RemoveCard => "remove_card",
        BoostedRewardOption.TransformCard => "transform_card",
        BoostedRewardOption.Potion2 => "potion2",
        BoostedRewardOption.MaxHp5 => "maxhp5",
        BoostedRewardOption.RandomRelic => "random_relic",
        BoostedRewardOption.EnchantCard => "enchant_card",
        BoostedRewardOption.UpgradeChoose1 => "upgrade_choose1",
        BoostedRewardOption.UpgradeRandom2 => "upgrade_random2",
        _ => "none",
    };

    // 未配置时的防御性出口：直接结束事件
    private Task Finish()
    {
        SetEventFinished(L10NLookup(Id.Entry + ".pages.INITIAL.description"));
        return Task.CompletedTask;
    }
}
