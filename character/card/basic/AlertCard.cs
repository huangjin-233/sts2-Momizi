using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using Momizi.character.relics;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
// ReSharper disable All

namespace Momizi.character.card.basic;

[RegisterCard(typeof(MomiziCardPool))]
[RegisterCharacterStarterCard(typeof(Momizi))]
public class AlertCard: ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 2;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Basic;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    public bool Attack = true;
    public bool Intnet;
    public bool Flag;

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/basic/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Jizhong",1 ),
        ModCardVars.Int("Dodge",10 ),
    ];

    protected override bool IsPlayable => Flag;
    //打出条件
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MomiziKeyword.Thinking,CardKeyword.Innate,CardKeyword.Retain,CardKeyword.Exhaust];
    public AlertCard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack)
        {
            Attack = false;
        }
        if (Intnet && Attack)
        {
            Flag = true;
        }
        else
        {
            Flag = false;
        }
    }
    // 打出时的效果逻辑
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        foreach (var creature in Owner.Creature.CombatState.HittableEnemies)
        {
            if (creature.Monster.IntendsToAttack || creature.Monster.NextMove.Intents.Any(intent => intent.IntentType == IntentType.Debuff))
            {
                Intnet = true;
            }
        }

        if (Intnet && Attack)
        {
            Flag = true;
        }
        else
        {
            Flag = false;
        }
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<JiZhong>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["Jizhong"].BaseValue, Owner.Creature, null);
    }
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        Attack = true;
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars["Dodge"].UpgradeValueBy(10);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromPower<JiZhong>(),
        HoverTipFactory.FromKeyword(MomiziKeyword.Debuff),
    ];
}