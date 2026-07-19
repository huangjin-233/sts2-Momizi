using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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
public class DexStrike: ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
       new CalculationBaseVar(9m),
       new ExtraDamageVar(3),
       new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) =>
           GetValue(card.Owner))
    ];
    public static Dictionary<Player, int> scry = new();
    public DexStrike() : base(energyCost, type, rarity, targetType)
    {
    }
    public static void SetValue(Player player, int value)
    {
        scry[player] = value;
    }

    public static int GetValue(Player player)
    {
        return scry.TryGetValue(player, out int value) ? value : 0;
    }

    public static void IncrementValue(Player player, int value)
    {
        scry[player] = value;
    }
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
            await DamageCmd.Attack(DynamicVars.CalculatedDamage)
                .FromCard(this) // 攻击来源
                .Targeting(cardPlay.Target) // 攻击目标
                .Execute(choiceContext); // 执行攻击效果
    }
    
    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(2);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromKeyword(MomiziKeyword.Scry)
    ];
}