using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Momizi.character.Mini;
using Momizi.character.power;
using MinionLib.Commands;
using MinionLib.Minion;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 援军：1费/技能/罕见 消耗6层：召唤1只白狼，若场上已有2只白狼，所有召唤物额外+6 HP。
// 升级：0费，消耗5层
[RegisterCard(typeof(MomiziCardPool))]
public class BeaconFire : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("AlertCost", 6),
        ModCardVars.Int("HpBonus", 6)
    ];

    protected override bool IsPlayable => Owner.Creature.GetPowerAmount<Alert>() >= 12;
    public BeaconFire() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<Alert>(choiceContext, Owner.Creature, -DynamicVars["AlertCost"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<BeaconFirePower>(choiceContext, Owner.Creature, 3, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlertCost"].UpgradeValueBy(-2);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromPower<BeaconFirePower>()
    ];
}