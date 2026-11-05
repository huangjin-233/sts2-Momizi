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

// 反击：1费/攻击/普通 造成6点伤害；若本回合获得过警戒，改为10点。升级：8/13
[RegisterCard(typeof(MomiziCardPool))]
public class CounterAttack : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/common/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        ModCardVars.Int("Bonus", 10)
    ];

    public CounterAttack() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.FindPower<Alert>()?.GainedThisTurn > 0)
        {
            await DamageCmd.Attack(DynamicVars["Bonus"].BaseValue)
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
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Bonus"].UpgradeValueBy(3);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Alert>()
    ];
}