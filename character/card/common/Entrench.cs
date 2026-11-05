using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Targeting;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.common;

// 盘踞：1费/技能/普通 —— 占位符，待实现
[RegisterCard(typeof(MomiziCardPool))]
public class Entrench : ModCardTemplate
{
    public override bool GainsBlock => true;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AllAllies;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/common/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
         new BlockVar(5m,ValueProp.Move)
    ];

    public Entrench() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);//防御
        if (CombatState != null)
        {
            IEnumerable<Creature> enumerable = from c in CombatState.GetTeammatesOf(Owner.Creature)
                where c.IsAlive && c.IsPlayer
                select c;
            foreach (Creature item in enumerable)
            {
                await CreatureCmd.GainBlock(item, DynamicVars.Block, cardPlay);
            }
        }

        foreach (var pet in cardPlay.Card.Owner.Creature.Pets)
        {
            if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
            {
                await CreatureCmd.GainBlock(pet, DynamicVars.Block, cardPlay);
                if (pet.GetPowerAmount<Coordination>() >= 3)
                {
                    await CreatureCmd.GainBlock(pet, DynamicVars.Block.IntValue+pet.GetPowerAmount<DexterityPower>(),ValueProp.Unpowered, cardPlay);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf),
    HoverTipFactory.FromKeyword(MomiziKeyword.Coordinations)];
}