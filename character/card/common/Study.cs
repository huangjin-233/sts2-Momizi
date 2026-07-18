using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character;
using Momizi.character.power;
using Momizi.character.relics;
using Momizi.character.Scry;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
// ReSharper disable All

namespace Momizi.character.cards.basic;

[RegisterCard(typeof(MomiziCardPool))]
public class Study: ModCardTemplate
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

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/common/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Autumn",2),
        new EnergyVar(2)
    ];
    public Study() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ScryCmd.Execute(new BlockingPlayerChoiceContext(), Owner.Creature.Player, 3);
        await CardPileCmd.Draw(choiceContext, 1, Owner);//抽卡
        IReadOnlyList<CardModel> cards1 = PileType.Draw.GetPile(Owner).Cards.Where((CardModel c) => c.Keywords.Contains(MomiziKeyword.Spell)).ToList();
        IReadOnlyList<CardModel> cards2 = PileType.Discard.GetPile(Owner).Cards.Where((CardModel c) => c.Keywords.Contains(MomiziKeyword.Spell)).ToList();
        IReadOnlyList<CardModel> cards3 = PileType.Hand.GetPile(Owner).Cards.Where((CardModel c) => c.Keywords.Contains(MomiziKeyword.Spell)).ToList();
        List<CardModel> cards = new List<CardModel>();
        cards.AddRange(cards1);
        cards.AddRange(cards2);
        cards.AddRange(cards3);
        foreach (CardModel study in await CardSelectCmd.FromSimpleGrid(choiceContext,cards, Owner, new CardSelectorPrefs(RelicModel.L10NLookup("从弃牌堆和抽牌堆选择一张牌加入手卡"), 1)))
        {
            CardCmd.Upgrade(study);
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromKeyword(MomiziKeyword.Scry),
        HoverTipFactory.FromKeyword(MomiziKeyword.Spell),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];
}