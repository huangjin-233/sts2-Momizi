#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.rare;
using Momizi.character.events;
using Momizi.character.Mini;
using Momizi.character.power;
using InferiorPower = Momizi.character.power.Inferior;
using Momizi.character.rest;
using Momizi.character.rewards;
using Momizi.character.Scry;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
#pragma warning disable CS0649 // 从未对字段赋值，字段将一直保持其默认值
namespace Momizi.character.relics;
[RegisterRelic(typeof(MomiziRelicPool))]
[RegisterCharacterStarterRelic(typeof(Momizi))]
public class Tengu: ModRelicTemplate,IRelicExtraIconAmountLabelSpecsProvider,IModRightClickableRelic
{
    // 稀有度
    public override RelicRarity Rarity => RelicRarity.Starter;

    private const int InferiorMaxStacks = 5;
    private int _inferior;
    private int _intelligence;
    private bool _eliteCombat;
    private bool _inferiorAppliedInCombat;
    // 天狗的认可：本场战斗内因天狗（右键大天狗）累计获得的自卑层数（持有该 buff 时上限 1）
    private int _tenguCappedInferior;
    // 选择一次道路/强化所需的固定情报数（原为 1，现为 3：每拥有 3 情报可选择一次道路）
    public const int IntelligenceCost = 3;
    // 额外奖励的【持久化载体】：RitsuLib 仅 relic/power/card 支持 [SavedProperty]，
    // 故说模式所需跨存档保留的奖励数据暂存于此；全部运行期状态与逻辑已迁入独立静态单例
    // BoostedRewardManager，此处不包含任何额外奖励逻辑。
    private int _pendingBoostedReward;
    private int _pendingBoostedRewardAmount;
    private string _pendingBoostedEnchantmentId = string.Empty;
    private int _pendingAlertConversionIntelligence;
    public bool Flag = true;
    public bool inferior;

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://images/relics/{GetType().Name}.png"
    );
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(16m,ValueProp.Unpowered),
    ];

    // 战斗中自卑层数变化 → 同步持久化计数（含右键、白狼死亡、战斗开始回填）
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not InferiorPower || !ReferenceEquals(power.Owner, Owner.Creature))
        {
            return;
        }

        int stacks = Math.Clamp(power.Amount, 0, InferiorMaxStacks);
        if (stacks != _inferior)
        {
            SetCharges(stacks);
        }
    }

    // 战斗结束：以当前自卑层数为准回写计数（覆盖“移除自卑”类效果导致的差额）；
    // 强化遭遇的三档基础等额外奖励由独立单例 BoostedRewardManager 结算。
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        int stacks = Owner.Creature.GetPowerAmount<Inferior>();
        if (stacks != _inferior)
        {
            SetCharges(stacks);
        }
        await BoostedRewardManager.AfterCombatEnd(Owner);
    }

    // 玩家回合开始：回填持久化的自卑；并结算警戒（敌怪有害意图 → 警戒）
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player)
        {
            return;
        }

        if (!_inferiorAppliedInCombat)
        {
            _inferiorAppliedInCombat = true;
            if (Inferior > 0)
            {
                await PowerCmd.Apply<Inferior>(new ThrowingPlayerChoiceContext(), Owner.Creature, Inferior, Owner.Creature, null);
            }
        }

        // 每当有敌怪意图对椛或白狼造成伤害、施加负面效果时获得警戒；精英/首领战斗 +2。同一次敌方行为只计一次。
        int gained = 0;
        foreach (var creature in combatState.Enemies)
        {
            if (!creature.IsAlive || creature.Monster?.NextMove == null)
            {
                continue;
            }
            bool harmful = creature.Monster.NextMove.Intents.Any(i =>
                i.IntentType is IntentType.Attack or IntentType.DeathBlow or IntentType.Debuff or IntentType.DebuffStrong or IntentType.CardDebuff);
            if (!harmful)
            {
                continue;
            }
            gained += _eliteCombat ? 2 : 1;
        }
        if (gained > 0)
        {
            await PowerCmd.Apply<Alert>(new ThrowingPlayerChoiceContext(), Owner.Creature, gained, Owner.Creature, null);
        }
    }

    public IReadOnlyList<ExtraIconAmountLabelSpec> GetRelicExtraIconAmountLabelSpecs()
    {
        return
        [
            ExtraIconAmountLabelSpec.Plain(
                ExtraIconAmountLabelCorner.BottomRight,
                _inferior.ToString()),
            ExtraIconAmountLabelSpec.Plain(
                ExtraIconAmountLabelCorner.TopRight,
                "情" + _intelligence),
        ];
    }//右上角为情报存量

    private void SetCharges(int value)
    {
        _inferior = value;
        InvokeDisplayAmountChanged();
    }

    public void AddIntelligence(int amount)
    {
        _intelligence += amount;
        InvokeDisplayAmountChanged();
    }

    public void SpendIntelligence(int amount)
    {
        _intelligence = Math.Max(0, _intelligence - amount);
        InvokeDisplayAmountChanged();
    }

    // 问号节点：花 3 情报（IntelligenceCost）把本幕随机事件替换为"两个候选事件二选一"
    // 一人选全队享：由"领头"（本遗物持有者）的情报产生抉择事件；选择结果全队共享
    public override EventModel ModifyNextEvent(EventModel currentEvent)
    {
        if (Owner == null || Owner.RunState is not RunState runState)
        {
            return currentEvent;
        }

        // 防御：随机池万一抽到未配置的抉择事件时，换成普通事件
        if (currentEvent is IntelligenceChoiceEvent unconfigured && !unconfigured.IsConfigured)
        {
            var fallbackPool = Owner.RunState.Act.AllEvents
                .Where(e => !runState.VisitedEventIds.Contains(e.Id) && e is not IntelligenceChoiceEvent)
                .ToList();
            return fallbackPool.Count > 0
                ? fallbackPool[runState.Rng.Niche.NextInt(fallbackPool.Count)]
                : currentEvent;
        }

        if (Intelligence < IntelligenceCost || currentEvent is IntelligenceChoiceEvent)
        {
            return currentEvent;
        }

        var visited = runState.VisitedEventIds;
        var pool = Owner.RunState.Act.AllEvents
            .Where(e => !ReferenceEquals(e, currentEvent))
            .Where(e => !visited.Contains(e.Id))
            .Where(e => e is not IntelligenceChoiceEvent)
            .ToList();
        if (pool.Count < 1)
        {
            return currentEvent; // 候选不足，走正常事件
        }

        // 问号节点二选一：保留"原事件" 或 花 3 情报换成 1 个新事件
        var newEvent = pool.UnstableShuffle(runState.Rng.Niche).First();
        var choiceEvent = ModelDb.Event<IntelligenceChoiceEvent>();
        choiceEvent.CandidateA = newEvent;
        choiceEvent.CandidateB = null;
        choiceEvent.FallbackEvent = currentEvent;
        choiceEvent.NormalEncounter = null;
        choiceEvent.Modifier = IntelligenceChoiceEvent.MonsterModifier.None;
        choiceEvent.DesignatedTengu = this; // 领头遗物：扣其持有者情报
        choiceEvent.IsConfigured = true;
        return choiceEvent;
    }

    // 强化遭遇：词条在战斗开始时对全体敌人生效（额外奖励系统，独立单例）
    public override async Task BeforeCombatStart()
    {
        _tenguCappedInferior = 0; // 每场战斗重置天狗认可的自卑累计上限
        await BoostedRewardManager.BeforeCombatStart(Owner);
    }

    public void SetPendingBoostedReward(IntelligenceChoiceEvent.BoostedRewardOption option, string? enchantmentId = null)
    {
        PendingBoostedReward = (int)option;
        PendingBoostedRewardAmount = option switch
        {
            IntelligenceChoiceEvent.BoostedRewardOption.Heal10 => 10,
            IntelligenceChoiceEvent.BoostedRewardOption.MaxHp5 => 5,
            _ => 0,
        };
        PendingBoostedEnchantmentId = enchantmentId ?? string.Empty;
    }

    public void QueueAlertConversionReward(int intelligence)
    {
        _pendingAlertConversionIntelligence += intelligence;
    }

    public void ClearPendingAlertConversionReward()
    {
        PendingAlertConversionIntelligence = 0;
    }

    [SavedProperty]
    public int PendingAlertConversionIntelligence
    {
        get => _pendingAlertConversionIntelligence;
        private set
        {
            AssertMutable();
            _pendingAlertConversionIntelligence = value;
        }
    }

    public void ClearPendingBoostedReward()
    {
        PendingBoostedReward = 0;
        PendingBoostedRewardAmount = 0;
        PendingBoostedEnchantmentId = string.Empty;
    }

    [SavedProperty]
    public int PendingBoostedReward
    {
        get => _pendingBoostedReward;
        private set
        {
            AssertMutable();
            _pendingBoostedReward = value;
        }
    }

    [SavedProperty]
    public int PendingBoostedRewardAmount
    {
        get => _pendingBoostedRewardAmount;
        private set
        {
            AssertMutable();
            _pendingBoostedRewardAmount = value;
        }
    }

    [SavedProperty]
    public string PendingBoostedEnchantmentId
    {
        get => _pendingBoostedEnchantmentId;
        private set
        {
            AssertMutable();
            _pendingBoostedEnchantmentId = value;
        }
    }

    // tier2 固定基础：把本战既有卡牌奖励的稀有度提升一档（RegularEncounter → EliteEncounter）
    public override CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
        => BoostedRewardManager.ModifyCardRewardCreationOptions(player, options);

    // 强化遭遇：奖励叠加（各档基础金币×2 + 抽选/警戒转化磁贴）。逻辑在独立单例
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
        => BoostedRewardManager.TryModifyRewards(Owner, player, rewards, room);

    [SavedProperty]
    public int Inferior
    {
        get
        {
            return _inferior;
        }
        private set
        {
            AssertMutable();
            if (_inferior != value)
            {
                _inferior = value;
                UpdateDisplay();
            }
        }
    }//自卑持久化
    [SavedProperty]
    public int Intelligence
    {
        get
        {
            return _intelligence;
        }
        private set
        {
            AssertMutable();
            if (_intelligence != value)
            {
                _intelligence = value;
                UpdateDisplay();
            }
        }
    }//情报持久化
    private void UpdateDisplay()
    {
        InvokeDisplayAmountChanged();
    }

    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        return Owner.Creature.GetPowerAmount<Alert>() >= 5;
    }

    public bool CanExecuteRightClick(ModRightClickExecutionContext context)
    {
        return Owner.Creature.GetPowerAmount<Alert>() >= 5;
    }

    // 右键执行（多人下会在所有客户端同步执行）
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (Owner.Creature.GetPowerAmount<Alert>() < 5)
        {
            return;
        }
        Flash();
        ICombatState? combatState = Owner.Creature.CombatState;
        bool strong = Owner.Creature.GetPowerAmount<Alert>() >= 12;
        // 天狗的威光：为大天狗伤害追加 Amount
        decimal glory = Owner.Creature.GetPowerAmount<TenguGloryPower>();
        int approval = (int)Owner.Creature.GetPowerAmount<TenguApprovalPower>();
        int inferiorGrant = strong ? 2 : 1;
        // 天狗的认可：本场战斗内，因天狗（右键大天狗）累计获取的自卑上限为 1 层；未持有该 buff 则按原值。
        if (approval > 0)
        {
            int remaining = 1 - _tenguCappedInferior;
            inferiorGrant = Math.Min(inferiorGrant, Math.Max(0, remaining));
            _tenguCappedInferior += inferiorGrant;
        }
        if (strong)
        {
            DynamicVars.Damage.BaseValue = 24 + glory;
        }
        else
        {
            DynamicVars.Damage.BaseValue = 16 + glory;
        }
        if (inferiorGrant > 0)
        {
            await PowerCmd.Apply<Inferior>(new ThrowingPlayerChoiceContext(), Owner.Creature, inferiorGrant, Owner.Creature, null);
        }
        await PowerCmd.Remove<Alert>(Owner.Creature);
        

        // 其他友军（白狼、队友；不包括椛）获得 2 力量、2 敏捷
        if (combatState != null)
        {
            var targets = new List<Creature>();
            foreach (var pet in Owner.Creature.Pets)
            {
                if (pet != Owner.Creature)
                {
                    targets.Add(pet);
                }
            }
            foreach (var player in combatState.Players)
            {
                if (player.Creature != Owner.Creature && !targets.Contains(player.Creature))
                {
                    targets.Add(player.Creature);
                }

                foreach (var playerpet in player.Creature.Pets)
                {
                    if (playerpet != Owner.Creature)
                    {
                        targets.Add(playerpet);
                    }
                }
            }
            foreach (var target in targets)
            {
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, 2, Owner.Creature, null);
                await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), target, 2, Owner.Creature, null);
            }
            // 天狗的认可：召唤/发动大天狗时，椛也获得相同的力量与敏捷
            if (approval > 0)
            {
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, approval, Owner.Creature, null);
                await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, approval, Owner.Creature, null);
            }
        }
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), combatState?.HittableEnemies, DynamicVars.Damage, Owner.Creature);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<Inferior>()
    ];
    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
        {
            return false;
        }
        options.Add(new InferiorRest(player));
        return true;
    }//火堆
    public void InferiorRest()
    {
        if (Flag)
        {
            Inferior--;
            Flag = false;
        }
    }//火堆效果

    public override Task BeforeRoomEntered(AbstractRoom room)
    {
        _eliteCombat = room.RoomType == RoomType.Elite || room.RoomType == RoomType.Boss;
        _inferiorAppliedInCombat = false;
        if (room.RoomType == RoomType.RestSite)
        {
            Flag = true;
            inferior = Inferior > 0;
        }
        return Task.CompletedTask;
    }
}

[HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.FromSerializable))]
internal static class TenguBoostedRewardRestorePatch
{
    private static void Postfix(CombatRoom __result, IRunState? runState)
    {
        if (runState == null)
        {
            return;
        }

        foreach (var player in runState.Players)
        {
            var tengu = player.GetRelic<Tengu>();
            if (tengu == null || (tengu.PendingBoostedReward <= 0
                && tengu.PendingAlertConversionIntelligence <= 0))
            {
                continue;
            }

            if (tengu.PendingBoostedReward > 0)
            {
                var reward = BoostedRewardManager.CreateBoostedReward(player,
                    (IntelligenceChoiceEvent.BoostedRewardOption)tengu.PendingBoostedReward,
                    string.IsNullOrEmpty(tengu.PendingBoostedEnchantmentId)
                        ? null
                        : tengu.PendingBoostedEnchantmentId);
                if (reward != null)
                {
                    __result.AddExtraReward(player, reward);
                }
            }
            if (tengu.PendingAlertConversionIntelligence > 0)
            {
                __result.AddExtraReward(player,
                    new IntelligenceReward(player, tengu.PendingAlertConversionIntelligence));
            }
        }
    }
}
