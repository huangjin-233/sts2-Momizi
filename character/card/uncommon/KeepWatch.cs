using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Targeting;
using Momizi.character.card.special;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 守望：1费/技能/罕见 —— 占位符，待实现  多人专属
[RegisterCard(typeof(MomiziCardPool))]
public class KeepWatch : ModCardTemplate
{
    public override bool GainsBlock => true;
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    public static Creature wolf;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
    new BlockVar(9m,ValueProp.Move)];

    public KeepWatch() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (wolf != null)
        {
            await PowerCmd.Remove<KeepWatchPower>(wolf);
        }
        wolf = cardPlay.Target;
        if (wolf != null)
        {
            await CreatureCmd.GainBlock(wolf, DynamicVars.Block, null);
        }

        if (CombatState != null)
        {
            var keep = CombatState.CreateCard<KeepWatchPlayer>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(keep, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf),
        HoverTipFactory.FromPower<KeepWatchPower>()
    ];
}