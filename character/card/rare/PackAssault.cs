using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Commands;
using Momizi.character.power;
using Momizi.character.single;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.rare;

// 群狼突袭：3费/攻击/稀有 —— 占位符，待实现
[RegisterCard(typeof(MomiziCardPool))]
public class PackAssault : ModCardTemplate
{
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/rare/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public PackAssault() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var pet in cardPlay.Card.Owner.Creature.Pets)
        {
            if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
            {
                int hits = pet.GetPowerAmount<Coordination>() / 2;
                await PowerCmd.Remove<Coordination>(pet);
                for (int i = 0; i < hits; i++)
                {
                    if (cardPlay.Target != null)
                    {
                        await MinionAnimCmd.PlayBumpAttackAsync(pet, cardPlay.Target);
                        await CreatureCmd.Damage(choiceContext, cardPlay.Target, WolfSingle.amount+Owner.Creature.GetPowerAmount<StrengthPower>(), ValueProp.Move, pet);
                        await PowerCmd.Remove<VigorPower>(pet);
                        Coordination.firstattack = true;
                    }
                    await Task.Delay(150);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf),
        HoverTipFactory.FromPower<Coordination>()
    ];
}