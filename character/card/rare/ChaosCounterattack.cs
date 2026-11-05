using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.special;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.rare;

public class ChaosCounterattack: ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 3;
    // 卡牌类型
    private const CardType type = CardType.Power;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AllEnemies;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/rare/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(4, ValueProp.Move)
    ];

    public ChaosCounterattack() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<Chaos>(choiceContext, Owner.Creature,1, Owner.Creature, null);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromPower<RainFallPower>(),
        HoverTipFactory.FromPower<JiZhong>(),
    ];
}