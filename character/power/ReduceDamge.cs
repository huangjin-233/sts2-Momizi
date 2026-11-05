#nullable enable
using System;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;


[RegisterPower]
public class ReduceDamge : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/ReduceDamge.png",
        BigIconPath: "res://images/powers/ReduceDamge.png"
    );
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
        {
            return amount;
        }
        return Math.Max(0m, amount - 9m);
    }
}