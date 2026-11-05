#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Momizi.character.card.rare;
using Momizi.character.card.uncommon;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 临时力量：Amount 即为当前临时力量层数，持续到本回合结束。
[RegisterPower]
public class Change : ModTemporaryAppliedPowerTemplate<MountainDen,StrengthPower>
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsPositive => false;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/Change.png",
        BigIconPath: "res://images/powers/Change.png"
    );
}