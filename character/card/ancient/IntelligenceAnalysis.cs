using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Momizi.character.power;
using Momizi.character.relics;
using Momizi.character.rewards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
// ReSharper disable All

namespace Momizi.character.card.ancient;

// 情报剖析：先古/技能/2费 —— 模板
// 本场战斗只可打出一次；消耗 3 点情报，并以本场战斗无法获得情报为代价，
// 使一名敌人的非攻击行为对我方全体均无效。
[RegisterCard(typeof(MomiziCardPool))]
public class IntelligenceAnalysis : ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 2;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Ancient;
    // 目标类型（指定一名敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 情报成本
    private const int IntelligenceCost = 3;

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/ancient/{GetType().Name}.png"
    );

    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];

    // 本场战斗只可打出一次：直接消耗（本卡 Exhaust 即每张副本仅一次）
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public IntelligenceAnalysis() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 场上情报 < 3 时不可打（自动回手，不消耗能量）
    protected override bool IsPlayable =>
        Owner.GetRelic<Tengu>() is { } tengu && tengu.Intelligence >= IntelligenceCost;

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        // 1) 消耗 3 点情报
        Owner.GetRelic<Tengu>()?.SpendIntelligence(IntelligenceCost);

        // 2) 本场战斗无法获得情报：置位战斗标记，抑制战斗结束警戒→情报转化
        BoostedRewardManager.LockIntelligenceThisCombat();

        // 3) 使目标敌人的非攻击行为对我方全体均无效
        await PowerCmd.Apply<IntelligenceAnalysisPower>(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<IntelligenceAnalysisPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];
}