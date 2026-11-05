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

namespace Momizi.character.card.common;

// 不退：1费/技能/普通 —— 模板，待实现
// 获得1层易伤、15点格挡、2层覆甲。升级：20点格挡、3层覆甲。
[RegisterCard(typeof(MomiziCardPool))]
public class StandFirm : ModCardTemplate
{
    public override bool GainsBlock => true;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/common/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(15, ValueProp.Move),
        new PowerVar<VulnerablePower>(1),
        ModCardVars.Int("Armor", 2)
    ];

    public StandFirm() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, DynamicVars.Vulnerable.BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, DynamicVars["Armor"].BaseValue,
            Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5);
        DynamicVars["Armor"].UpgradeValueBy(1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<PlatingPower>()
    ];
}