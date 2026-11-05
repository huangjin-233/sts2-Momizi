using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 传令：1费/技能/罕见 消耗6层：召唤物+2力量；警戒≥12时 消耗6层：召唤物+2力量+3覆甲；
// 升级：消耗5层，3力
[RegisterCard(typeof(MomiziCardPool))]
public class Relay : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("AlertCost", 6),
        new PowerVar<StrengthPower>(1),
        ModCardVars.Int("Armor", 5),
        ModCardVars.Int("Thresh", 8)
    ];

    public Relay() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var alert = Owner.Creature.GetPowerAmount<Alert>();
        var cost = DynamicVars["AlertCost"].BaseValue;
        if (alert < cost)
        {
            return;
        }
        bool strong = alert >= DynamicVars["Thresh"].IntValue;
        await PowerCmd.Apply<Alert>(choiceContext, Owner.Creature, -cost, Owner.Creature, this);

        var strength = DynamicVars.Strength.BaseValue;
        var armor = DynamicVars["Armor"].BaseValue;
        foreach (var pet in Owner.Creature.Pets)
        {
            if (!pet.IsAlive)
            {
                continue;
            }
            await PowerCmd.Apply<StrengthPower>(choiceContext, pet, strength, Owner.Creature, this);
            // 覆甲（Plating）：赋予召唤物Plating，使其每回合开始获得等额格挡
            if (strong)
            {
                await PowerCmd.Apply<WolfPlatingPower>(choiceContext, pet, armor, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlertCost"].UpgradeValueBy(-1);
        DynamicVars.Strength.UpgradeValueBy(2);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf),
        HoverTipFactory.FromPower<WolfPlatingPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];
}