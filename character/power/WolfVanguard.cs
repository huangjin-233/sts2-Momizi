#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 白狼护主：敌方单体攻击在椛/白狼间随机选目标。
/// 回合开始时（意图已生成）预判一次目标，显示在目标身上（受击前显示），并在命中时兑现。
/// 白狼上场即有（由 Wolf.OnSummon 挂载）；若开启「掩护」命令会与之相互抵消，但不会死循环。
/// 白狼被攻击时的格挡结算由 WolfBlockFix（Transpiler 改写 CreatureCmd.Damage）保证使用狼自身格挡。
/// </summary>
[RegisterPower]
public class WolfVanguard : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/WolfVanguard.png",
        BigIconPath: "res://images/powers/WolfVanguard.png"
    );

    // 本回合预判的受击目标（椛或白狼）；无敌方单体攻击时为 null
    private Creature? _intendedReceiver;
    // 上一次标记的生物，用于下回合清除
    private Creature? _marked;

    // 上个回合敌人是否对任一白狼造成过实际（未格挡）伤害 —— 供「铁壁」等判定
    public static bool AnyWolfDamagedLastTurn;
    private static bool _anyWolfDamagedThisTurn;

    // 本回合该狼是否已因「造成伤害」/「获得格挡」触发过同调 +1（每回合每类型至多一次，按狼独立）
    private bool _coordAttackDone;
    private bool _coordBlockDone;

    // 玩家回合开始：清除上一回合标记，并按敌方单体攻击意图预判本回合目标
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player)
        {
            return;
        }

        // 交接回合标记：本回合的受伤记录转为"上个回合"状态，并清零本回合计数
        AnyWolfDamagedLastTurn = _anyWolfDamagedThisTurn;
        _anyWolfDamagedThisTurn = false;
        // 重置本回合同调触发门控
        _coordAttackDone = false;
        _coordBlockDone = false;

        // 清除上一回合的预命中标记
        if (_marked != null)
        {
            await PowerCmd.Remove<AimedAt>(_marked);
            _marked = null;
        }
        _intendedReceiver = null;

        Creature? owner = Owner.PetOwner?.Creature;
        if (Owner.IsDead || owner == null)
        {
            return;
        }

        // 本回合是否有敌怪会发动单体攻击
        bool enemySingleAttacks = combatState.Enemies.Any(e =>
            e.IsAlive && e.Monster?.NextMove != null &&
            e.Monster.NextMove.Intents.OfType<SingleAttackIntent>().Any());

        if (!enemySingleAttacks)
        {
            return;
        }

        // 预判目标：在椛/白狼间 50% 均等随机
        if (Owner.PetOwner != null)
        {
            int target = Owner.PetOwner.RunState.Rng.CombatCardGeneration.NextInt(0, 2);
            if (target != 0)
            {
                _intendedReceiver = owner;
            }
            else
            {
                _intendedReceiver = Owner;
            }
        }

        // 受击前显示：将标记作用到预判目标身上
        if (_intendedReceiver != null)
        {
            await PowerCmd.Apply<AimedAt>(new ThrowingPlayerChoiceContext(), _intendedReceiver, 1, owner, null);
            _marked = _intendedReceiver;
        }
    }

    // 命中时兑现预判：若目标是白狼，则将椛受到的敌方伤害改由白狼承受
    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        // 仅敌方来源、目标为椛（白狼主人）的伤害
        if (dealer?.IsEnemy != true || target != Owner.PetOwner?.Creature)
        {
            return target;
        }
        // 白狼死亡或无实际伤害不转移
        if (Owner.IsDead || amount <= 0)
        {
            return target;
        }
        // 兑现：预判目标是白狼时才转移
        if (_intendedReceiver != Owner)
        {
            return target;
        }

        if (Owner.HasPower<YanhuPower>())
        {
            return Owner.PetOwner.Creature;
        }
        // 结算时标记承受方：白狼受击抖动
        Owner.GetCreatureNode()?.AnimShake();
        return Owner;
    }
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
        {
            return amount;
        }

        if (Math.Floor(amount - Owner.Block) < 0)
            return 0;
        return Math.Floor(amount - Owner.Block);
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        // 敌人对白狼造成实际（未格挡）伤害 → 记录本回合白狼受伤（供「铁壁」判定）
        if (target == Owner && dealer?.IsEnemy == true && result.UnblockedDamage > 0)
        {
            _anyWolfDamagedThisTurn = true;
        }

        // 白狼的格挡由引擎（CreatureCmd.Damage → DamageBlockInternal）自动结算，
        // 这里不再手动重算 Block，避免结算后二次扣减导致「打击后防御消失」。
    }

    // 白狼造成伤害 → 同调 +1（每回合至多一次，按狼独立）
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer,
        DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || _coordAttackDone || !Owner.IsAlive)
        {
            return;
        }
        _coordAttackDone = true;
        await PowerCmd.Apply<Coordination>(choiceContext, Owner, 1, Owner.PetOwner?.Creature, null);
    }

    // 白狼获得格挡 → 同调 +1（每回合至多一次，按狼独立）
    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props,
        CardModel? cardSource)
    {
        if (creature != Owner || _coordBlockDone || !Owner.IsAlive)
        {
            return;
        }
        _coordBlockDone = true;
        await PowerCmd.Apply<Coordination>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner.PetOwner?.Creature, null);
    }
}
