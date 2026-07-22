#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Momizi.character.rest;
using Momizi.character.Scry;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;
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
    // 情报系统：小怪节点强化词条（选择后待生效 / 当前战斗生效）
    private IntelligenceChoiceEvent.MonsterModifier _pendingMonsterModifier;
    private IntelligenceChoiceEvent.MonsterModifier _activeMonsterModifier;
    public static bool Flag = true;
    public static bool inferior;

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://images/relics/{GetType().Name}.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(16m,ValueProp.Move)
    ];

    // 战斗中自卑层数变化 → 同步持久化计数（含右键、白狼死亡、战斗开始回填）
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is Inferior)
        {
            int stacks = (int)Math.Clamp(power.Amount, 0, InferiorMaxStacks);
            if (stacks != _inferior)
            {
                SetCharges(stacks);
            }
        }
    }

    // 战斗结束：以当前自卑层数为准回写计数（覆盖“移除自卑”类效果导致的差额）
    public override Task AfterCombatEnd(CombatRoom room)
    {
        int stacks = Owner.Creature.GetPowerAmount<Inferior>();
        if (stacks != _inferior)
        {
            SetCharges(stacks);
        }
        return Task.CompletedTask;
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

    public void SetPendingMonsterModifier(IntelligenceChoiceEvent.MonsterModifier modifier)
    {
        _pendingMonsterModifier = modifier;
    }

    // 问号节点：花 1 情报把本幕随机事件替换为"两个候选事件二选一"
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

        if (Intelligence < 1 || currentEvent is IntelligenceChoiceEvent)
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

        // 问号节点二选一：保留“原事件” 或 花 1 情报换成 1 个新事件
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

    // 强化遭遇：词条在战斗开始时对全体敌人生效
    public override async Task BeforeCombatStart()
    {
        if (_pendingMonsterModifier == IntelligenceChoiceEvent.MonsterModifier.None)
        {
            return;
        }
        _activeMonsterModifier = _pendingMonsterModifier;
        _pendingMonsterModifier = IntelligenceChoiceEvent.MonsterModifier.None;

        ICombatState? combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }
        var enemies = combatState.HittableEnemies.ToList();
        var ctx = new ThrowingPlayerChoiceContext();
        switch (_activeMonsterModifier)
        {
            case IntelligenceChoiceEvent.MonsterModifier.HpBoost:
                foreach (var enemy in enemies)
                {
                    await CreatureCmd.SetMaxHp(enemy, enemy.MaxHp * 1.5m);
                    await CreatureCmd.Heal(enemy, enemy.MaxHp * 0.5m, false);
                }
                break;
            case IntelligenceChoiceEvent.MonsterModifier.Strength:
                foreach (var enemy in enemies)
                {
                    await PowerCmd.Apply<StrengthPower>(ctx, enemy, 2, enemy, null);
                }
                break;
            case IntelligenceChoiceEvent.MonsterModifier.OpeningBlock:
                foreach (var enemy in enemies)
                {
                    await CreatureCmd.GainBlock(enemy, 5m, ValueProp.Move, null, false);
                }
                break;
        }
    }

    // 强化遭遇：奖励加成（金币 ×2 / 遗物 / 药水）
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (_activeMonsterModifier == IntelligenceChoiceEvent.MonsterModifier.None || room is not CombatRoom combatRoom)
        {
            return false;
        }
        var modifier = _activeMonsterModifier;
        _activeMonsterModifier = IntelligenceChoiceEvent.MonsterModifier.None; // 只用一次
        switch (modifier)
        {
            case IntelligenceChoiceEvent.MonsterModifier.HpBoost: // 金币 ×2
                rewards.Add(new GoldReward(combatRoom.Encounter.MinGoldReward, combatRoom.Encounter.MaxGoldReward, player));
                break;
            case IntelligenceChoiceEvent.MonsterModifier.Strength: // 金币 ×2 + 随机遗物
                rewards.Add(new GoldReward(combatRoom.Encounter.MinGoldReward, combatRoom.Encounter.MaxGoldReward, player));
                rewards.Add(new RelicReward(player));
                break;
            case IntelligenceChoiceEvent.MonsterModifier.OpeningBlock: // 药水掉落 +50%
                if (Random.Shared.NextDouble() < 0.5)
                {
                    rewards.Add(new PotionReward(player));
                }
                break;
        }
        return false;
    }

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

    // 右键执行（多人下会在所有客户端同步执行）
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        Flash();
        ICombatState? combatState = Owner.Creature.CombatState;
        bool strong = Owner.Creature.GetPowerAmount<Alert>() >= 12;
        DynamicVars.Damage.BaseValue = strong ? 24 : 16;
        await PowerCmd.Apply<Inferior>(new ThrowingPlayerChoiceContext(), Owner.Creature, strong ? 2 : 1, Owner.Creature, null);

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
                if (player.Creature != null && player.Creature != Owner.Creature && !targets.Contains(player.Creature))
                {
                    targets.Add(player.Creature);
                }
            }
            foreach (var target in targets)
            {
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, 2, Owner.Creature, null);
                await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), target, 2, Owner.Creature, null);
            }
        }
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), combatState?.HittableEnemies, DynamicVars.Damage, Owner.Creature);
        await PowerCmd.Remove<Alert>(Owner.Creature);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
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
