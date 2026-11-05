#nullable enable
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

// 以守为攻：每当你（椛）获得格挡时，白狼获得1层活力 —— 模板，待实现
[RegisterPower]
public class CounterstancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/CounterstancePower.png",
        BigIconPath: "res://images/powers/CounterstancePower.png"
    );

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props,
        CardModel? cardSource)
    {
        if (creature == Owner)
        {
            foreach (var pet in Owner.Pets)
            {
                if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
                {
                    await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), pet, Amount, Owner, null);
                }
            }
        }
    }
}