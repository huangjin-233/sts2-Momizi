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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.special;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

[RegisterCard(typeof(MomiziCardPool))]
public class ChewOverAndSavor: ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 2;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    public bool Intnet;
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );
    protected override bool IsPlayable => Intnet;
    public override int MaxUpgradeLevel => 99;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
      new CalculationBaseVar(9),
      new ExtraDamageVar(2),
      new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => card.Owner.Creature.GetPowerAmount<JiZhong>()),
    ];
    public ChewOverAndSavor() : base(energyCost, type, rarity, targetType)
    {
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        foreach (var creature in Owner.Creature.CombatState.HittableEnemies)
        {
            if (creature.Monster.IntendsToAttack)
                Intnet = true;
        }
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.CalculatedDamage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
        await PowerCmd.Apply<Spell>(choiceContext, Owner.Creature, 3, Owner.Creature, null);
        await PowerCmd.Apply<ChewOver>(choiceContext, cardPlay.Target, Owner.Creature.GetPowerAmount<JiZhong>()/2, Owner.Creature, null);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, Owner.Creature.GetPowerAmount<JiZhong>()/2, Owner.Creature, null);
    }
    
    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(4);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromPower<Spell>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];
}