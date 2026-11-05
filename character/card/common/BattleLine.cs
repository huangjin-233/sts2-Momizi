using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.common;

// 阵线：1费/技能/普通 —— 模板，待实现
// 获得8格挡；每有一只白狼，你和白狼各额外获得3格挡。升级：11格挡。
[RegisterCard(typeof(MomiziCardPool))]
public class BattleLine : ModCardTemplate
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
        new BlockVar(8, ValueProp.Move),
        ModCardVars.Int("PerWolf", 3)
    ];

    public BattleLine() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        foreach (var pet in cardPlay.Card.Owner.Creature.Pets)
        {
            if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
            {
                await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["PerWolf"].BaseValue,ValueProp.Unpowered ,cardPlay);
                await CreatureCmd.GainBlock(pet, DynamicVars["PerWolf"].BaseValue,ValueProp.Unpowered, cardPlay);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf)
    ];
}