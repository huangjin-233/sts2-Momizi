using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Targeting;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 猎获：1费/能力/罕见 —— 占位符，待实现
[RegisterCard(typeof(MomiziCardPool))]
public class Hunt : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;
    
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    public static bool upgrade = false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public Hunt() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
        {
            if (IsUpgraded) 
                await PowerCmd.Apply<HuntPowered>(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
            else
                await PowerCmd.Apply<HuntPower>(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf)
    ];
}