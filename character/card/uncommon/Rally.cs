using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 集结号：1费/技能/罕见/消耗 获得4层警戒。升级：6层
[RegisterCard(typeof(MomiziCardPool))]
public class Rally : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("AlertGain", 4),
        ModCardVars.Int("DrawPer", 1)
    ];

    public Rally() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<Alert>(choiceContext, Owner.Creature, DynamicVars["AlertGain"].BaseValue, Owner.Creature, this);

        // 场上每有一名友军（含白狼及队友）抽 DrawPer 张牌
        // 友军 = 椛自身 + 我的白狼随从 + 队友单位（Allies 不含自身/己方随从，需分别统计）
        int allies = 0;
        foreach (var pet in Owner.Creature.Pets)
        {
            if (pet.IsAlive)
            {
                allies++;
            }
        }
        var alliesSide = Owner.Creature.CombatState?.Allies;
        if (alliesSide != null)
        {
            foreach (var ally in alliesSide)
            {
                if (ally.IsAlive)
                {
                    allies++;
                }
            }
        }
        int draw = (int)DynamicVars["DrawPer"].BaseValue * allies;
        if (draw > 0)
        {
            await CardPileCmd.Draw(choiceContext, draw, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlertGain"].UpgradeValueBy(2);
        DynamicVars["DrawPer"].UpgradeValueBy(1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Alert>(),
        
    ];
}