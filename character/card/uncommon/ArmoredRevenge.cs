using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 铁甲反击：2费/攻击/罕见 —— 模板，待实现
// 获得10点格挡，然后造成等同于你当前格挡值的伤害
[RegisterCard(typeof(MomiziCardPool))]
public class ArmoredRevenge : ModCardTemplate
{
    public override bool GainsBlock => true;
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(10, ValueProp.Move),
        new DamageVar(0m,ValueProp.Move)
    ];

    public ArmoredRevenge() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        DynamicVars.Damage.BaseValue = Owner.Creature.Block;
        if (cardPlay.Target != null)
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this,cardPlay).Targeting(cardPlay.Target)
                .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];
}