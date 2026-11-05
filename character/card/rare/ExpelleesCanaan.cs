using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.special;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.rare;
[RegisterCard(typeof(MomiziCardPool))]
public class ExpelleesCanaan: ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 4;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/rare/{GetType().Name}.png"
    );
    public override int MaxUpgradeLevel => 9999;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MomiziKeyword.Spell,CardKeyword.Retain];
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(1),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => card.Owner.Creature.GetPowerAmount<JiZhong>()/4),
        ModCardVars.Int("JiZhong",5)
    ];

    public ExpelleesCanaan() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override bool IsPlayable => Owner.Creature.GetPowerAmount<Alert>() >= Alert.max/2;
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.CalculatedDamage.BaseValue)
            .WithHitCount(15)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
        await CreatureCmd.Stun(cardPlay.Target);
        await PowerCmd.Apply<Spell>(choiceContext, Owner.Creature, 4, Owner.Creature, null);
        await PowerCmd.Apply<JiZhong>(choiceContext, Owner.Creature,-DynamicVars["JiZhong"].BaseValue,Owner.Creature,null);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(1);
        if (DynamicVars["JiZhong"].BaseValue < 0)
        {
            DynamicVars["JiZhong"].UpgradeValueBy(-2);
        }
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromPower<Spell>(),
        HoverTipFactory.FromPower<JiZhong>(),
        HoverTipFactory.FromKeyword(MomiziKeyword.Spell)
    ];
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
        List<CardModel> ExpelleesCanaan = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            ExpelleesCanaan.Add(combatState.CreateCard<ExpelleesCanaan>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(ExpelleesCanaan, PileType.Hand, owner);
        return ExpelleesCanaan;
    }
    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (autoPlayType == AutoPlayType.None)
            return true;
        return false;
    }
}