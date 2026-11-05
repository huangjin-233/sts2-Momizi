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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.common;

// 巡逻：1费/技能/普通 获得8格挡。若本回合有敌怪意图攻击，获得1层警戒。升级：11格挡/2层
[RegisterCard(typeof(MomiziCardPool))]
public class Patrol : ModCardTemplate
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
        ModCardVars.Int("AlertGain", 1),
        ModCardVars.Int("Vigor",3)
    ];

    private static bool IsHarmful(AbstractIntent intent)
    {
        return intent.IntentType is IntentType.Attack or IntentType.DeathBlow
            or IntentType.Debuff or IntentType.DebuffStrong
            or IntentType.CardDebuff or IntentType.StatusCard;
    }

    public Patrol() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        bool threatened = Owner.Creature.CombatState != null && Owner.Creature.CombatState.Enemies.Any(e =>
            e.IsAlive && e.Monster?.NextMove != null && e.Monster.NextMove.Intents.Any(IsHarmful));
        if (threatened)
        {
            await PowerCmd.Apply<Alert>(choiceContext, Owner.Creature, DynamicVars["AlertGain"].BaseValue, Owner.Creature, this);
            foreach (var pet in cardPlay.Card.Owner.Creature.Pets)
            {
                if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
                {
                    await PowerCmd.Apply<VigorPower>(choiceContext, pet, DynamicVars["Vigor"].BaseValue, Owner.Creature,
                        this);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars["AlertGain"].UpgradeValueBy(1);
        DynamicVars["Vigor"].UpgradeValueBy(2);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromPower<VigorPower>()
    ];
}