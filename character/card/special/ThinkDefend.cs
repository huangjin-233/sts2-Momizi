using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.cards.basic;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.special;

[RegisterCard(typeof(MomiziCardPool))]
public class ThinkDefend : ModCardTemplate, QuickThinking.IChoosable
{
    // 基础耗能
    private const int energyCost = -1;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Status;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/basic/{GetType().Name}.png"
    );

    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4, ValueProp.Move)
    ];

    public ThinkDefend() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    public async Task OnChosen()
    {
        await PowerCmd.Apply<ThinkDefendPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature,
            null);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
}