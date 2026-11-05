// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Watcher, Version=1.5.2.0, Culture=neutral, PublicKeyToken=null
// Watcher.Code.Commands.ScryCmd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace Momizi.character.Scry;

public static class ScryCmd
{
	// 从整个抽牌堆中随机抽取 count 张（预见改为随机查看，而非看顶部）。
	// 用 run 的 RNG（Niche）保证可复现；无 run 上下文时退回随机器。
	private static readonly Dictionary<Player, List<CardModel>> _seenCardsByPlayer = [];
	private static readonly Dictionary<Player, int> _seenCardsResetTurnByPlayer = [];

	public static void ResetSeenCards(Player player)
	{
		int turnNumber = player.PlayerCombatState?.TurnNumber ?? -1;
		if (_seenCardsResetTurnByPlayer.TryGetValue(player, out int lastResetTurn) && lastResetTurn == turnNumber)
		{
			return;
		}
		_seenCardsByPlayer.Remove(player);
		_seenCardsResetTurnByPlayer[player] = turnNumber;
	}

	private static List<CardModel> PickRandom(Player player, IReadOnlyList<CardModel> all, int count)
	{
		List<CardModel> pool = new List<CardModel>(all);
		List<CardModel> result = new List<CardModel>(count);
		var niche = player.RunState?.Rng?.Niche;
		while (result.Count < count && pool.Count > 0)
		{
			int index = niche != null ? niche.NextInt(pool.Count) : System.Random.Shared.Next(pool.Count);
			result.Add(pool[index]);
			pool.RemoveAt(index);
		}
		return result;
	}
	public static Task<ScryResult> Execute(BlockingPlayerChoiceContext choiceContext, CardModel card,int amount)
	{
		return Execute(choiceContext, card.Owner, amount);
	}
	public static Task<ScryResult> Lock(BlockingPlayerChoiceContext choiceContext, CardModel card,int amount)
	{
		return Lock(choiceContext, card.Owner, amount);
	}

	public static async Task<ScryResult> Execute(BlockingPlayerChoiceContext choiceContext, Player player, int amount)
	{
		IEnumerable<IModifyScryAmount> modifiers;
		int modifiedAmount = ScryHook.ModifyScryAmount(player, amount, out modifiers);
		await ScryHook.AfterModifyingScryAmount(choiceContext, player, modifiers, amount, modifiedAmount);
		if (modifiedAmount <= 0)
		{
			_seenCardsByPlayer.Remove(player);
			return default(ScryResult);
		}
		CardPile pile = PileType.Draw.GetPile(player);
		CardPile discardPile = PileType.Discard.GetPile(player);
		ICombatState combatState = player.Creature.CombatState;
		if (combatState == null)
		{
			_seenCardsByPlayer.Remove(player);
			return default(ScryResult);
		}
		List<CardModel> seenCards = PickRandom(player, pile.Cards, modifiedAmount);
		_seenCardsByPlayer[player] = seenCards;
		if (seenCards.Count == 0)
		{
			_seenCardsByPlayer.Remove(player);
			return default(ScryResult);
		}
		CardSelectorPrefs prefs = new CardSelectorPrefs(RelicModel.L10NLookup("查看抽牌堆顶部"), 0);
		List<CardModel> cardsToDiscard = (await CardSelectCmd.FromSimpleGrid(choiceContext, seenCards, player, prefs)).ToList();
		discardPile.InvokeContentsChanged();
		await ScryHook.AfterScryed(choiceContext, player, modifiedAmount, cardsToDiscard.Count, cardsToDiscard);
		return new ScryResult(cardsToDiscard);
	}

	public static async Task<ScryResult> Lock(BlockingPlayerChoiceContext choiceContext, Player player, int amount)
	{
		IEnumerable<IModifyScryAmount> modifiers;
		int modifiedAmount = ScryHook.ModifyScryAmount(player, amount, out modifiers);
		await ScryHook.AfterModifyingScryAmount(choiceContext, player, modifiers, amount, modifiedAmount);
		if (modifiedAmount <= 0)
		{
			return default(ScryResult);
		}
		CardPile pile = PileType.Draw.GetPile(player);
		CardPile discardPile = PileType.Discard.GetPile(player);
		ICombatState combatState = player.Creature.CombatState;
		if (combatState == null)
		{
			return default(ScryResult);
		} ;
		if (!_seenCardsByPlayer.TryGetValue(player, out List<CardModel>? seenCards) || seenCards.Count == 0)
		{
			return default(ScryResult);
		}

		CardSelectorPrefs prefs = new CardSelectorPrefs(RelicModel.L10NLookup("选择一张牌至于牌堆顶部"), 1);
		List<CardModel> cardsToDiscard = (await CardSelectCmd.FromSimpleGrid(choiceContext, seenCards, player, prefs)).ToList();
		discardPile.InvokeContentsChanged();
		List<CardModel> lockedCards = (await CardSelectCmd.FromSimpleGrid(choiceContext, cardsToDiscard, player, new CardSelectorPrefs(RelicModel.L10NLookup("选择一张牌至于牌堆顶部"), 1))).ToList();
		foreach (CardModel cardModel in lockedCards)
			if (cardModel != null)
			{
				await CardPileCmd.Add(cardModel, PileType.Hand);
				cardModel.SetToFreeThisTurn();
				seenCards.Remove(cardModel);
			}
		if (seenCards.Count == 0)
		{
			_seenCardsByPlayer.Remove(player);
		}
		await ScryHook.AfterScryed(choiceContext, player, modifiedAmount, cardsToDiscard.Count, cardsToDiscard);
		return new ScryResult(cardsToDiscard);
	}
}
