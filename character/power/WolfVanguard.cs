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
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 白狼护主：敌方单体攻击在椛/白狼间随机选目标。
/// 回合开始时（意图已生成）预判一次目标，显示在目标身上（受击前显示），并在命中时兑现。
/// 白狼上场即有（由 Wolf.OnSummon 挂载）；若开启「掩护」命令会与之相互抵消，但不会死循环。
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
    // 无「掩护」时，白狼作为原始目标受击应使用自己的格挡；这里记录被游戏默认误扣前的主人格挡。
    private int? _ownerBlockBeforeSummonDamage;

    // 玩家回合开始：清除上一回合标记，并按敌方单体攻击意图预判本回合目标
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player)
        {
            return;
        }

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
        _intendedReceiver = Random.Shared.NextDouble() < 0.5 ? Owner : owner;

        // 受击前显示：将标记作用到预判目标身上
        await PowerCmd.Apply<AimedAt>(new ThrowingPlayerChoiceContext(), _intendedReceiver, 1, owner, null);
        _marked = _intendedReceiver;
    }

    public override Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        var owner = Owner.PetOwner?.Creature;
        _ownerBlockBeforeSummonDamage = target == Owner && owner is { IsAlive: true } && !HasCover()
            ? owner.Block
            : null;
        return Task.CompletedTask;
    }

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        var owner = Owner.PetOwner?.Creature;
        if (target != Owner || owner == null || _ownerBlockBeforeSummonDamage == null)
        {
            return amount;
        }

        decimal ownerBlockedDamage = Math.Max(_ownerBlockBeforeSummonDamage.Value - owner.Block, 0);
        if (ownerBlockedDamage > 0)
        {
            owner.GainBlockInternal(ownerBlockedDamage);
        }

        _ownerBlockBeforeSummonDamage = null;
        decimal damageBeforeOwnerBlock = amount + ownerBlockedDamage;
        decimal summonBlockedDamage = Owner.DamageBlockInternal(damageBeforeOwnerBlock, props);
        return Math.Max(damageBeforeOwnerBlock - summonBlockedDamage, 0);
    }

    private bool HasCover()
    {
        return Owner.Powers.Any(power => power is YanhuPower);
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
        // 结算时标记承受方：白狼受击抖动
        Owner.GetCreatureNode()?.AnimShake();
        return Owner;
    }
}
