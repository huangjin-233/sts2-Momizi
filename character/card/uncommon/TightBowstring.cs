using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 绷紧的弓弦：1费/能力/罕见 警戒溢出时，每溢出2层获得1点临时力量。升级：每溢1层
[RegisterCard(typeof(MomiziCardPool))]
public class TightBowstring : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Threshold", 2),
        ModCardVars.Int("Stren",1)
    ];

    public TightBowstring() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        if (IsUpgraded)
        {
            await PowerCmd.Apply<TightBowstringPowered>(choiceContext, Owner.Creature, DynamicVars["Stren"].BaseValue,
                Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<TightBowstringPower>(choiceContext, Owner.Creature, DynamicVars["Stren"].BaseValue,
                Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Threshold"].UpgradeValueBy(-1);
        DynamicVars["Stren"].UpgradeValueBy(1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];
}