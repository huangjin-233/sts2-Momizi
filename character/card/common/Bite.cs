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
using Momizi.character.single;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.common;

// 撕咬：1费/攻击/普通 —— 占位符，待实现
[RegisterCard(typeof(MomiziCardPool))]
public class Bite : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/common/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1)
    ];

    public Bite() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
        foreach (var pet in Owner.Creature.Pets)
        {
            if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
            {
                await CreatureCmd.Damage(choiceContext, cardPlay.Target,
                    WolfSingle.amount + Owner.Creature.GetPowerAmount<StrengthPower>(), ValueProp.Move, pet);
                await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, 1,
                    pet, this);

                if (pet.GetPowerAmount<Coordination>() >= 3)
                {
                    await Task.Delay(200);
                    await CreatureCmd.Damage(choiceContext, cardPlay.Target,
                        WolfSingle.amount + Owner.Creature.GetPowerAmount<StrengthPower>(), ValueProp.Move, pet);
                    await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, 1,
                        pet, this);
                }
                await Task.Delay(300);
                await PowerCmd.Remove<VigorPower>(pet);
                Coordination.firstattack = true;
                await PowerCmd.Remove<VigorPower>(Owner.Creature);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    HoverTipFactory.FromPower<VulnerablePower>(),
    HoverTipFactory.FromKeyword(MomiziKeyword.Coordinations)
    ];
}