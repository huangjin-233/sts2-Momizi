#nullable enable
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 不屈：当你的格挡被一次攻击打破时，该次攻击不会使你致死，层数-1。
/// </summary>
[RegisterPower]
public class UnyieldingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/UnyieldingPower.png",
        BigIconPath: "res://images/powers/UnyieldingPower.png"
    );

    // 抵挡标记：确保同一次伤害只触发一次抵挡（格挡结算与消耗分处于两个钩子阶段）
    private bool _preventedDeath;

    // 致死判定：必须在 ModifyHpLostAfterOsty 阶段执行，此时格挡已结算、伤害值确定
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || _preventedDeath || Amount <= 0)
        {
            return amount;
        }

        // 仅在伤害为正、且会导致当前生命值归零时触发抵挡
        if (amount > 0 && amount >= Owner.CurrentHp)
        {
            _preventedDeath = true;
            // 精确保留 1 HP（而非其他固定值）
            return Math.Max(0m, Owner.CurrentHp - 1m);
        }
        return amount;
    }

    // 抵挡结算后消耗层数；层数为此前积攒的剩余生命
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || !_preventedDeath)
        {
            return;
        }
        _preventedDeath = false;
        if (Amount <= 1)
        {
            await PowerCmd.Remove(this);
        }
        else
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -1, dealer, cardSource);
        }
    }
}