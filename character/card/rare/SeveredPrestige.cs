using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.rare;

// 首级威仪：2费/能力/稀有
// 每回合首次攻击时对目标施加1层看破；白狼攻击时对目标施加1层虚弱；
// 白狼对带看破的目标伤害+Amount（Amount=5）。升级：费用降为1。
[RegisterCard(typeof(MomiziCardPool))]
public class SeveredPrestige : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/rare/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Bonus", 5)
    ];

    public SeveredPrestige() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Amount = 白狼对看破目标的额外伤害（5）；虚弱固定1层在 power 内发放。
        await PowerCmd.Apply<SeveredPrestigePower>(choiceContext, Owner.Creature,
            DynamicVars["Bonus"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1); // 2 → 1 费
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<ForesightPower>(),
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf),
        HoverTipFactory.FromPower<WeakPower>()
    ];
}