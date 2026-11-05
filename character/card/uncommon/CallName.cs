using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Targeting;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 呼名：1费/技能/罕见 —— 占位符，待实现
[RegisterCard(typeof(MomiziCardPool))]
public class CallName : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Coordination",4)    
    ];

    public CallName() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
            await PowerCmd.Apply<Coordination>(choiceContext, cardPlay.Target, DynamicVars["Coordination"].BaseValue,
                Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Coordination"].UpgradeValueBy(2);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Coordination>(),
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf)
    ];
}