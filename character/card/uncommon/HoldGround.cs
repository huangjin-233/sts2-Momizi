using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 死守：1费/技能/罕见 消耗全部：每层获得2格挡。升级：每层3格挡
[RegisterCard(typeof(MomiziCardPool))]
public class HoldGround : ModCardTemplate
{
    public override bool GainsBlock => true;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];

    public HoldGround() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var alert = Owner.Creature.GetPowerAmount<Alert>();
        int plating = alert ;
        if (alert <= 0)
        {
            return;
        }
        if (alert >= 10)
        {
            await PowerCmd.Apply<Alert>(choiceContext, Owner.Creature, 10, Owner.Creature, this);
            plating = 10;
        }
        else
        {
            await PowerCmd.Remove<Alert>(Owner.Creature);
        }
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, plating, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromPower<PlatingPower>()
    ];
}