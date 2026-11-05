#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 默契：仅对所指定的白狼生效——
// 该白狼攻击时，你获得 Amount(3) 点格挡；你攻击时，该白狼获得 Amount×2/3(2) 点格挡。
[RegisterPower]
public class TacitPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/TacitPower.png",
        BigIconPath: "res://images/powers/TacitPower.png"
    );

    // 被「默契」指定的那只白狼（随从的主人引用取 Owner.PetOwner）
    public static Creature? ChosenWolf;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer,
        DamageResult result, ValueProp props,
        Creature target, CardModel? cardSource)
    {
        if (ChosenWolf == null || !ChosenWolf.IsAlive)
        {
            return;
        }
        // 该白狼攻击 → 椛获得格挡
        if (dealer == ChosenWolf)
        {
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
        }
        // 椛攻击 → 该白狼获得格挡
        if (dealer == Owner)
        {
            await CreatureCmd.GainBlock(ChosenWolf, Amount / 3 * 2, ValueProp.Unpowered, null);
        }
    }
}