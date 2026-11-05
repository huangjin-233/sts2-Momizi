using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MinionLib.Targeting;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.rare;

// 同仇敌忾：2费/能力/稀有 —— 占位符，待实现
[RegisterCard(typeof(MomiziCardPool))]
public class JointHatred : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/rare/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public JointHatred() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
            await PowerCmd.Apply<JointHartredPower>(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
    }
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf)
    ];
}