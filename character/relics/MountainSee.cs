#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using Momizi.character.power;
using Momizi.character.Scry;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.relics;
[RegisterRelic(typeof(MomiziRelicPool))]
[RegisterCharacterStarterRelic(typeof(Momizi))]
[RegisterTouchOfOrobasRefinement(typeof(TenguSee))]
public class MountainSee: ModRelicTemplate,IModRightClickableRelic
{
    // 稀有度
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://images/relics/{GetType().Name}.png"
    );
    // 每回合开始时，抽一张牌
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            Flash(); // 触发遗物图标闪烁
            await ScryCmd.Execute(new BlockingPlayerChoiceContext(), Owner.Creature.Player, 2);
        }
    }
    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        return Owner.Creature.GetPowerAmount<Alert>() >= Alert.max/2;
    }

    // 右键执行（多人下会在所有客户端同步执行）
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        Flash();
        IReadOnlyList<CardModel> cards1 = PileType.Draw.GetPile(Owner).Cards;
        List<CardModel> cards = new List<CardModel>();
        cards.AddRange(cards1);
        foreach (CardModel see in await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(),cards, Owner, new CardSelectorPrefs(L10NLookup("从抽牌堆选择一张牌加入手卡"), 1)))
        {
            await CardPileCmd.Add(see, PileType.Hand);
            see.EnergyCost.AddThisCombat(1);
        }
        await CardPileCmd.Add(await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(L10NLookup("选择一张牌返回抽卡堆"), 1), context: new BlockingPlayerChoiceContext(), player: Owner, filter: null, source: this), PileType.Draw);
        await PowerCmd.Remove<Alert>(Owner.Creature);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromKeyword(MomiziKeyword.Scry),
    ];
}