using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.special;

[RegisterCard(typeof(TokenCardPool))]
public class RabiesBite: ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Status;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    public override int MaxUpgradeLevel => 9999;
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/special/{GetType().Name}.png"
    );
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MomiziKeyword.Spell,CardKeyword.Retain];
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9m, ValueProp.Move),
        new CardsVar(1)
    ];
    public RabiesBite() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Cards.IntValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
    public static async Task<CardModel> CreateInHand(Player owner, ICombatState combatState)
    {
        return (await CreateInHand(owner, 1, combatState)).FirstOrDefault();
    }
    public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, ICombatState combatState)
    {
        if (count == 0)
        {
            return Array.Empty<CardModel>();
        }
        if (CombatManager.Instance.IsOverOrEnding)
        {
            return Array.Empty<CardModel>();
        }
        List<CardModel> RabiesBite = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            RabiesBite.Add(combatState.CreateCard<RabiesBite>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(RabiesBite, PileType.Hand, owner);
        return RabiesBite;
    }
    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (autoPlayType == AutoPlayType.None)
            return true;
        return false;
    }
}