using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

[RegisterCard(typeof(MomiziCardPool))]
public class AdaptToCircumstances: ModCardTemplate
{
    protected override bool HasEnergyCostX => true;
    public override bool GainsBlock => true;
    // 基础耗能
    private const int energyCost = -1;
    // 卡牌类型
    private const CardType type = CardType.Skill;
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
        new BlockVar(5m, ValueProp.Move),
    ];

    public AdaptToCircumstances() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int num = ResolveEnergyXValue()-1;
        if (IsUpgraded)
        {
            num++;
        }
        if (cardPlay.Target.Monster.IntendsToAttack ||
            cardPlay.Target.Monster.NextMove.Intents.Any(intent => intent.IntentType == IntentType.Debuff))
        {
            for (int i = 0; i < num; i++)
            {
                await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            }
        }
        else
        {
            CardPile pile = PileType.Discard.GetPile(Owner);
            IEnumerable<CardModel> enumerable = pile.Cards
                .Where(c => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList()
                .StableShuffle(Owner.RunState.Rng.Shuffle)
                .Take(ResolveEnergyXValue());
            foreach (CardModel item in enumerable)
            {
                if (!CombatManager.Instance.IsOverOrEnding)
                {
                    if (item.TargetType == TargetType.AnyEnemy)
                    {
                        Creature target = Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
                        await CardCmd.AutoPlay(choiceContext, item, target);
                    }
                    else
                    {
                        await CardCmd.AutoPlay(choiceContext, item, null);
                    }
                    continue;
                }
                break;
            }
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromKeyword(MomiziKeyword.Debuff),
        HoverTipFactory.FromPower<ArtifactPower>()
    ];
}