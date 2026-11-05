#nullable enable
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.Scry;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class GrindInPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/GrindInPower.png",
        BigIconPath: "res://images/powers/GrindInPower.png"
    );

    private int strength;
    private int coordination;
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (power is Coordination)
        {
            coordination = Owner.GetPowerAmount<Coordination>()/2*Amount;
            if (coordination != strength)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -strength, Owner, null);
                strength = coordination*Amount;
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, strength, Owner, null);
            }
        }
    }
}