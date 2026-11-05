using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Targeting;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 并肩：1费/技能/罕见 —— 占位符，待实现
[RegisterCard(typeof(MomiziCardPool))]
public class FightSideBySide : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.ComputedBlock("Blocks", 0, (card, target) => card.DynamicVars["Blocks"].BaseValue + (target?.GetPowerAmount<Coordination>()*2 ?? 0)),
    ];

    public FightSideBySide() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
        {
            await PowerCmd.Apply<Coordination>(choiceContext, cardPlay.Target, 4, Owner.Creature, this);
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.EvaluateValueOrDefault("Blocks",target: cardPlay.Target),ValueProp.Unpowered, null);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf),
        HoverTipFactory.FromKeyword(MomiziKeyword.Coordinations)
    ];
}