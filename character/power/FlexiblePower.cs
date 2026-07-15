using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Momizi.character.card.uncommon;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace YakumoAkai.character.power;
[RegisterPower]
public sealed class FlexiblePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
    public int Num = 0;
    public override async Task AfterEnergyReset(Player player)
    {
        if (player == Owner.Player)
        {
            Num++;
            if (Num >= Flexible.flex)
            {
                await PowerCmd.Apply<Dodge>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
                Num = 0;
            }
        }
    }
}