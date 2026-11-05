#nullable enable
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 浑然一体：白狼额外造成等同其同调层数÷2 的伤害。
// 挂在每只白狼身上；Amount 未参与计算（白狼自带 Coordination）。
[RegisterPower]
public class OneWithAllPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/OneWithAllPower.png",
        BigIconPath: "res://images/powers/OneWithAllPower.png"
    );

    // 该白狼攻击时，额外造成 同调层数/2 点伤害
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (props.IsPoweredAttack() && dealer == Owner)
        {
            return Owner.GetPowerAmount<Coordination>() / 2;
        }
        return 0m;
    }
}