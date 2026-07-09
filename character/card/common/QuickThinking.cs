using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.special;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

// ReSharper disable All

namespace Momizi.character.cards.basic;

[RegisterCard(typeof(MomiziCardPool))]
public class QuickThinking: ModCardTemplate
{
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
    protected static readonly IReadOnlyList<CardModel> _choiceSets = new CardModel[2]
    {
        ModelDb.Card<ThinkAttack>(),
        ModelDb.Card<ThinkDefend>()
    };
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/basic/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(4, ValueProp.Move)
    ];

    public QuickThinking() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    public interface IChoosable
    {
        Task OnChosen();
    }
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
        List<CardModel> card = new List<CardModel>();
        foreach (var choice in _choiceSets)
        {
            CardModel mutableCard = this.CombatState.CreateCard((CardModel) choice, Owner);
            card.Add(mutableCard);
        }
        CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, card, Owner,false);
        if (cardModel == null)
            return;
        await ((IChoosable)cardModel).OnChosen();
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
}