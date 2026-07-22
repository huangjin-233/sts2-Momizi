#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Momizi.character.Scry;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class LiaowangPower : ModPowerTemplate, IModifyScryAmount
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/LiaowangPower.png",
        BigIconPath: "res://images/powers/LiaowangPower.png"
    );

    // 你的预见额外 +Amount（升级后 Amount 更大）。
    public int ModifyScryAmount(Player player, int amount)
    {
        // 此能力附着在白狼身上，效果仍属于它的主人。
        return player == Owner.PetOwner ? amount + (int)Amount : amount;
    }

    public Task AfterModifyingScryAmount(PlayerChoiceContext ctx, Player player, int originalAmount, int modifiedAmount)
    {
        return Task.CompletedTask;
    }
}
