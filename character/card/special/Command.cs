#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.special;

[RegisterCard(typeof(MomiziCardPool))]
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
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

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

    protected override bool IsPlayable => !hasPlayedThisTurn;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        hasPlayedThisTurn = true;

        CardModel liaowang = CombatState!.CreateCard<LiaowangCommand>(Owner);
        CardModel jiebei = CombatState.CreateCard<JiebeiCommand>(Owner);
        CardModel yanhu = CombatState.CreateCard<YanhuCommand>(Owner);

        if (IsUpgraded)
        {
            liaowang.UpgradeInternal();
            jiebei.UpgradeInternal();
            yanhu.UpgradeInternal();
        }

        List<CardModel> commands = new() { liaowang, jiebei, yanhu };
        CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, commands, Owner, false);
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

    protected override PileType GetResultPileTypeForCardPlay()
    {
        return PileType.Hand;
    }
}