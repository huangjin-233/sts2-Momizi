using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.special;

[RegisterCard(typeof(TokenCardPool))]
public class Shock: ModCardTemplate
{
    // 基础耗能
    private const int energyCost = -1;
    // 卡牌类型
    private const CardType type = CardType.Curse;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Curse;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    public override int MaxUpgradeLevel => 0;
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/special/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    public Shock() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.LoseBlock(Owner.Creature, 2);
    }
}