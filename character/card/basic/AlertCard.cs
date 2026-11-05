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
using MegaCrit.Sts2.Core.Models.Powers;
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
    public override bool GainsBlock => true;
    // 基础耗能
    private const int energyCost = 1;
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
        ModCardVars.Int("Alert",3 ),
        new BlockVar(6m,ValueProp.Move)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain,CardKeyword.Exhaust];
    public AlertCard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);//防御
        await PowerCmd.Apply<Alert>(choiceContext, Owner.Creature, DynamicVars["Alert"].BaseValue, Owner.Creature, null);
    }
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        Attack = true;
    }

    // 玩家回合开始：若敌方本回合有负面意图（攻击/死亡打击/施加负面），本卡本回合费用为 0
    // （机制同原版 BulletTime/Invoke：通过 EnergyCost.SetThisTurn 把本卡本回合费用压到 0，回合结束自动还原）
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && EnemyHasHarmfulIntent(combatState))
        {
            SetToFreeThisTurn();
        }
        await Task.CompletedTask;
    }

    // 敌方是否存在负面意图（含攻击/死亡打击/施加负面）
    private bool EnemyHasHarmfulIntent(ICombatState combatState)
    {
        foreach (var creature in combatState.HittableEnemies)
        {
            if (!creature.IsAlive || creature.Monster?.NextMove == null)
            {
                continue;
            }
            if (creature.Monster.NextMove.Intents.Any(i =>
                i.IntentType is IntentType.Attack or IntentType.DeathBlow or IntentType.Debuff or IntentType.DebuffStrong or IntentType.CardDebuff))
            {
                return true;
            }
        }
        return false;
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars["Alert"].UpgradeValueBy(3);
        DynamicVars.Block.UpgradeValueBy(3);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromKeyword(MomiziKeyword.Debuff)
    ];
}