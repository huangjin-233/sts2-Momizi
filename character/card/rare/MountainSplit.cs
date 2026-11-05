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

namespace Momizi.character.card.rare;

// 崩山：2费/攻击/稀有 —— 模板，待实现
// 造成10点伤害；若你的格挡≥15，改为造成22点伤害。升级：14/28。
[RegisterCard(typeof(MomiziCardPool))]
public class MountainSplit : ModCardTemplate
{
    public override bool GainsBlock => true;
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/rare/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(22,ValueProp.Move),
        ModCardVars.Damage("EmpoweredDamage", 36),
    ];

    public MountainSplit() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.Block > 15)
        {
            await DamageCmd.Attack(DynamicVars["EmpoweredDamage"].BaseValue)
                .FromCard(this,cardPlay)
                .Targeting(cardPlay.Target!)
                .Execute(choiceContext);
        }
        else
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this,cardPlay)
                .Targeting(cardPlay.Target!)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["EmpoweredDamage"].UpgradeValueBy(6);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];
}