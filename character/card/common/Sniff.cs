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
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.common;

// 嗅探：1费/技能/普通/消耗 每有一名敌人，获得1层警戒。召唤物获得2活力。升级：固有，3活力
[RegisterCard(typeof(MomiziCardPool))]
public class Sniff : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,CardKeyword.Innate];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/common/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Vigor", 2),
    ];

    public Sniff() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 每有一名存活敌人，获得1层警戒
        int enemyCount = Owner.Creature.CombatState.Enemies.Count(e => e.IsAlive);
        await PowerCmd.Apply<Alert>(choiceContext, Owner.Creature, enemyCount, Owner.Creature, this);

        // 所有召唤物获得活力
        int vigor = DynamicVars["Vigor"].IntValue;
        foreach (var pet in cardPlay.Card.Owner.Creature.Pets)
        {
            if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
            {
                await PowerCmd.Apply<VigorPower>(choiceContext, pet, vigor, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Vigor"].UpgradeValueBy(1);
        EnergyCost.UpgradeBy(-1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf)
    ];
}