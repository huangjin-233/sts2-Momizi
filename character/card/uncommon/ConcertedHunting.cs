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

namespace Momizi.character.card.uncommon;

// 协同狩猎：2费/攻击/罕见 —— 占位符，待实现
[RegisterCard(typeof(MomiziCardPool))]
public class ConcertedHunting : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
     new DamageVar(16m,ValueProp.Move)
    ];

    public ConcertedHunting() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hits = 0;
        if (cardPlay.Target != null)
        {
            foreach (var pet in Owner.Creature.Pets)
            {
                if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
                {
                    if (pet.GetPowerAmount<Coordination>() >= 5)
                    {
                        hits += 2;
                        for (int i = 0; i < hits; i++)
                        {
                            await CreatureCmd.Damage(choiceContext, cardPlay.Target,
                                WolfSingle.amount + Owner.Creature.GetPowerAmount<StrengthPower>(), ValueProp.Move,
                                pet);
                            await PowerCmd.Remove<VigorPower>(pet);
                            Coordination.firstattack = true;
                        }
                    }
                }
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this,cardPlay)
                    .Targeting(cardPlay.Target!)
                    .Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    HoverTipFactory.FromKeyword(MomiziKeyword.Coordinations)
    ];
}