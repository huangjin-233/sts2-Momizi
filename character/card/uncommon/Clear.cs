using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

[RegisterCard(typeof(MomiziCardPool))]
public class Clear: ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyAlly;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );
    //多人联机卡牌
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public Clear() : base(energyCost, type, rarity, targetType)
    {
    }
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 5, Owner);//抽卡
        await PowerCmd.Remove<Alert>(Owner.Creature);
        List<CardModel> list = PileType.Hand.GetPile(Owner).Cards.Where(c => c.IsTransformable && c.Type == CardType.Status).ToList();
        foreach (CardModel item in list)
        {
            var combatState = CombatState;
            if (combatState != null)
            {
               await CardCmd.Exhaust(choiceContext,item);
               await PowerCmd.Apply<StrengthPower>(choiceContext,Owner.Creature,1,Owner.Creature,null);
            }
        }
    }
    
    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromPower<Alert>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];
}