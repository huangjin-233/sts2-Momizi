#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Momizi.character.relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.events;

/// <summary>
/// 情报抉择事件：
/// - 问号节点：花 1 情报，从本幕两个候选事件中二选一（可放弃，直接进入原事件）；
/// - 小怪节点：普通遭遇 vs 强化遭遇（随机一个词条，词条与奖励可见）。
/// 由白狼天狗遗物在进入节点前通过 ModifyNextEvent / CreateRoom 补丁配置本事件。
/// </summary>
[RegisterSharedEvent]
public class IntelligenceChoiceEvent : ModEventTemplate
{
    /// <summary>小怪节点强化词条。</summary>
    public enum MonsterModifier
    {
        None = 0,
        /// <summary>全部敌人 HP +50% → 奖励：金币 ×2</summary>
        HpBoost = 1,
        /// <summary>全部敌人力量 +2 → 奖励：金币 ×2、随机普通遗物</summary>
        Strength = 2,
        /// <summary>敌人开局获得 5 格挡 → 奖励：药水掉落 +50%</summary>
        OpeningBlock = 3,
    }

    // ---- 进入节点前由遗物配置 ----
    public EventModel? CandidateA;
    public EventModel? CandidateB;
    public EventModel? FallbackEvent;        // 问号节点：放弃时进入的原事件
    public EncounterModel? NormalEncounter;  // 小怪节点：普通遭遇
    public MonsterModifier Modifier;         // 小怪节点：强化词条
    public Tengu? DesignatedTengu;           // 小怪节点：多人时“领头”玩家的遗物（一人选全队享）
    public bool IsConfigured;

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

    // 每次进入事件时随机选一张图（问号/小怪两个真实场景）；未配置走占位图 none。
    // 注意：AssetProfile 可能先于 OnRoomEnter 被读取，因此这里也要兜底随机，避免显示成 none。
    public override EventAssetProfile AssetProfile
    {
        get
        {
            bool configured = (CandidateA != null && CandidateB != null) || NormalEncounter != null;
            if (configured && _portrait == null)
            {
                _portrait = EventImages[Random.Shared.Next(EventImages.Length)];
            }
            return new EventAssetProfile(InitialPortraitPath: configured ? _portrait! : "res://images/events/none.png");
        }
    }

    // 每次进入事件时清空已选图，下次读取时重新随机
    public override void OnRoomEnter()
    {
        _portrait = null;
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
                new EventOption(this, Leave, FallbackEvent.Title, FallbackEvent.Title,
                    OptionKey("original"), Array.Empty<IHoverTip>()),
                new EventOption(this, () => ChooseCandidate(CandidateA), CandidateA.Title, CandidateA.Title,
                    OptionKey("candidate_a"), Array.Empty<IHoverTip>()),
            ];
        }

        // 小怪节点：普通 vs 强化（强化词条随节点随机）
        if (NormalEncounter != null)
        {
            return
            [
                new EventOption(this, ChooseNormalCombat, OptionKey("normal")),
                new EventOption(this, ChooseBoostedCombat, OptionKey(BoostedOptionSuffix())),
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
            tengu?.SpendIntelligence(1);
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
        // 本节点被拦截成事件后，战斗房间压栈不经过普通房间入口，
        // 若不手动推进遭遇游标，下一场战斗会重复出现相同怪物。
        if (Owner?.RunState is RunState runState)
        {
            runState.Act.MarkRoomVisited(RoomType.Monster);
        }
        EnterCombatWithoutExitingEvent(NormalEncounter!.ToMutable(), Array.Empty<Reward>(), shouldResumeAfterCombat: false);
        return Task.CompletedTask;
    }

    // 强化遭遇：扣 1 情报（只扣一次，防重复）；词条由遗物在战斗开始时施放，奖励由遗物加成。
    // 扣的是“领头”玩家（多人：一人选全队享），单人回退到 Owner。
    private Task ChooseBoostedCombat()
    {
        Tengu? tengu = DesignatedTengu ?? Owner?.GetRelic<Tengu>();
        if (!_intelligenceSpent && tengu != null)
        {
            _intelligenceSpent = true;
            tengu.SpendIntelligence(1);
            tengu.SetPendingMonsterModifier(Modifier);
        }
        return ChooseNormalCombat();
    }

    // ---- 工具 ----

    private string OptionKey(string suffix) => Id.Entry + ".pages.INITIAL.options." + suffix;

    private string BoostedOptionSuffix() => Modifier switch
    {
        MonsterModifier.HpBoost => "boosted_hp",
        MonsterModifier.Strength => "boosted_strength",
        MonsterModifier.OpeningBlock => "boosted_block",
        _ => "boosted",
    };

    // 未配置时的防御性出口：直接结束事件
    private Task Finish()
    {
        SetEventFinished(L10NLookup(Id.Entry + ".pages.INITIAL.description"));
        return Task.CompletedTask;
    }
}
