using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
// ReSharper disable All

namespace Momizi.character.cards.basic;

[RegisterCard(typeof(MomiziCardPool))]
public class AllReady: ModCardTemplate
{
    public override bool GainsBlock => true;
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/basic/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6m,ValueProp.Move)
    ];

    public AllReady() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Remove<Dodge>(Owner.Creature);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);//防御
        foreach (var creature in Owner.Creature.CombatState.HittableEnemies)
        {
            if (creature.Monster.IntendsToAttack)
            {
                int dodge = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 100);
                if (dodge <= 5 + Owner.Creature.GetPowerAmount<JiZhong>() * 5 - Owner.Creature.GetPowerAmount<Inferior>() * 10)
                {
                    await PowerCmd.Apply<Dodge>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                }
            }

            if (creature.Monster.NextMove.Intents.Any(intent => intent.IntentType == IntentType.Debuff))
            {
                int dodge = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 100);
                if (dodge <= 5 + Owner.Creature.GetPowerAmount<JiZhong>() * 5 - Owner.Creature.GetPowerAmount<Inferior>() * 10)
                {
                    await PowerCmd.Apply<Dodge>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                }
            }
        }

        await PowerCmd.Apply<Ready>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];
}