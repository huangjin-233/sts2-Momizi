#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MinionLib.Targeting;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.special;

[RegisterCard(typeof(TokenCardPool))]
public class Command : ModCardTemplate
{
    // 命令选项：由具体命令卡实现。
    public interface IChoosable
    {
        Task OnChosen();
    }

    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/special/{GetType().Name}.png"
    );

    // 仅通过白狼卫队生成，不进入随机生成池。
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    private bool hasPlayedThisTurn;

    public Command() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public static Creature? wolf;
    protected override bool IsPlayable => !hasPlayedThisTurn;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        hasPlayedThisTurn = true;
        wolf = cardPlay.Target;
        CardModel liaowang = CombatState!.CreateCard<LiaowangCommand>(Owner);
        CardModel jiebei = CombatState.CreateCard<JiebeiCommand>(Owner);
        CardModel yanhu = CombatState.CreateCard<YanhuCommand>(Owner);
        CardModel qianzhi = CombatState.CreateCard<QianzhiCommand>(Owner);

        if (IsUpgraded)
        {
            liaowang.UpgradeInternal();
            jiebei.UpgradeInternal();
            yanhu.UpgradeInternal();
            qianzhi.UpgradeInternal();
        }

        List<CardModel> commands = new() { liaowang, jiebei, yanhu, qianzhi };
        // 引擎的 FromChooseACardScreen 只支持 ≤2 张；改用 FromSimpleGrid 网格选卡以展示四张并选一。
        CardModel? selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext, commands, Owner,
            new CardSelectorPrefs(RelicModel.L10NLookup("选择一张命令"), 1))).FirstOrDefault();
        if (selected is IChoosable choosable)
        {
            await choosable.OnChosen();
        }
    }

    public override Task AfterEnergyReset(Player player)
    {
        hasPlayedThisTurn = false;
        return Task.CompletedTask;
    }

    protected override CardLocation GetResultLocationForCardPlay()
    {
        CardLocation resultLocationForCardPlay = base.GetResultLocationForCardPlay();
        if (resultLocationForCardPlay.pileType == PileType.Discard)
        {
            resultLocationForCardPlay.pileType = PileType.Hand;
        }
        return resultLocationForCardPlay;
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<JiebeiCommand>(IsUpgraded),
        HoverTipFactory.FromCard<LiaowangCommand>(IsUpgraded),
        HoverTipFactory.FromCard<YanhuCommand>(IsUpgraded),
        HoverTipFactory.FromCard<QianzhiCommand>(IsUpgraded)
    ];
}