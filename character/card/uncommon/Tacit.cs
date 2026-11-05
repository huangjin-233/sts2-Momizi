using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Targeting;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 默契：2费/技能/罕见 —— 选择一只白狼，只能在其同调≥6时打出；
// 该白狼攻击时椛获得3格挡，椛攻击时该白狼获得2格挡，消耗其6层同调。
[RegisterCard(typeof(MomiziCardPool))]
public class Tacit : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Tacit", 3)
    ];

    public Tacit() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    // 场上存在同调≥6 的存活白狼时才可打出
    protected override bool IsPlayable =>
        Owner.Creature.Pets.Any(p =>
            p.IsAlive && p.GetPowerAmount<WolfVanguard>() > 0 && p.GetPowerAmount<Coordination>() >= 6);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var wolf = cardPlay.Target;
        if (wolf == null || !wolf.IsAlive || wolf.GetPowerAmount<WolfVanguard>() <= 0 || wolf.GetPowerAmount<Coordination>() < 6)
        {
            // 无效目标：退还费用并回到手牌
            await PlayerCmd.GainEnergy(energyCost, Owner);
            await CardPileCmd.Add(cardPlay.Card, PileType.Hand, CardPilePosition.Random, cardPlay.Card, false);
            return;
        }

        // 指定该白狼，施加默契效果
        TacitPower.ChosenWolf = wolf;
        await PowerCmd.Apply<TacitPower>(choiceContext, Owner.Creature, DynamicVars["Tacit"].BaseValue, Owner.Creature, this);
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