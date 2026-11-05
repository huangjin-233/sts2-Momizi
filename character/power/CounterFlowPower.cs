#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.rare;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;


[RegisterPower]
public class CounterFlowPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/CounterFlowPower.png",
        BigIconPath: "res://images/powers/CounterFlowPower.png"
    );

    private decimal blockgain;
    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature == Owner)
        {
            while (blockgain >= CounterFlow.blocked)
            {
                blockgain -= amount;
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
            }
        }
    }
}