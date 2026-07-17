#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.relics;
using Momizi.character.Scry;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;
[RegisterPower]
public class PlanningPower : ModPowerTemplate,IAfterScryed
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Single;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/PlanningPower.png",
        BigIconPath: "res://images/powers/PlanningPower.png"
    );

    public async Task AfterScryed(PlayerChoiceContext ctx, Player player, int scryAmount, int discardedAmount, IEnumerable<CardModel> discarded)
    {
        List<CardModel> cards = discarded.ToList();
        Owner.Player?.RunState.Rng.Shuffle.Shuffle<CardModel>(cards);
        CardModel card = cards[0];
        card.EnergyCost.AddUntilPlayed(-1);
    }
}