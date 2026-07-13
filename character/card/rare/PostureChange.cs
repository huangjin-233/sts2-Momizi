using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.special;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.rare;

[RegisterCard(typeof(MomiziCardPool))]
public class PostureChange: ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 3;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    public static int posture = 0;
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/rare/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(4, ValueProp.Move)
    ];
    protected static readonly IReadOnlyList<CardModel> Chose = new CardModel[3]
    {
        ModelDb.Card<PostureChangeAttack>(),
        ModelDb.Card<PostureChangeSkill>(),
        ModelDb.Card<PostureChangeNormal>()
    };
    public PostureChange() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    public interface IChoosable
    {
        Task OnChosen();
    }
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> card = new List<CardModel>();
        foreach (var choice in Chose)
        {
            if (posture == 1 && choice == ModelDb.Card<PostureChangeAttack>() ||
                posture == 2 && choice == ModelDb.Card<PostureChangeSkill>() ||
                posture == 3 && choice == ModelDb.Card<PostureChangeNormal>())
            {
                continue;
            }
            CardModel mutableCard = CombatState.CreateCard((CardModel) choice, Owner);
            card.Add(mutableCard);
        }
        CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, card, Owner,false);
        if (cardModel == null)
            return;
        await ((IChoosable)cardModel).OnChosen();
    }
    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType resultPileType = base.GetResultPileTypeForCardPlay();
        if (resultPileType != PileType.Discard)
        {
            return resultPileType;
        }
        return PileType.Draw;
    }
    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromCard<PostureChangeAttack>(),
        HoverTipFactory.FromCard<PostureChangeSkill>(),
        HoverTipFactory.FromCard<PostureChangeNormal>(),
    ];
}