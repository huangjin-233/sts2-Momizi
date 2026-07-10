using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

[RegisterCard(typeof(MomiziCardPool))]
public class AutoBlock: ModCardTemplate
{
    public override bool GainsBlock => true;
    // 基础耗能
    private const int energyCost = 2;
    // 卡牌类型
    private const CardType type = CardType.Power;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/basic/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(0.5m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel _, Creature? target) => Owner.Creature.GetPowerAmount<Alert>())
    ];

    public AutoBlock() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BlockPosture>(choiceContext, Owner.Creature, DynamicVars.CalculatedBlock.BaseValue, Owner.Creature, null);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromPower<BlockPosture>(),
    ];
}