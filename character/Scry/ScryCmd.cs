// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Watcher, Version=1.5.2.0, Culture=neutral, PublicKeyToken=null
// Watcher.Code.Commands.ScryCmd
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

public static class ScryCmd
{
	public static Task<ScryResult> Execute(PlayerChoiceContext choiceContext, CardModel card,int amount)
	{
		return Execute(choiceContext, card.Owner, amount);
	}

	public static async Task<ScryResult> Execute(PlayerChoiceContext choiceContext, Player player, int amount)
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
		}
		List<CardModel> list = pile.Cards.Take(modifiedAmount).ToList();
		if (list.Count == 0)
		{
			return default(ScryResult);
		}
		CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, list.Count);
		List<CardModel> cardsToDiscard = (await CardSelectCmd.FromSimpleGrid(choiceContext, list, player, prefs)).ToList();
		foreach (CardModel card in cardsToDiscard)
		{
			await CardPileCmd.Add(card, discardPile);
			CombatManager.Instance.History.CardDiscarded(combatState, card);
			await Hook.AfterCardDiscarded(combatState, choiceContext, card);
		}
		discardPile.InvokeContentsChanged();
		await ScryHook.AfterScryed(choiceContext, player, modifiedAmount, cardsToDiscard.Count, cardsToDiscard);
		return new ScryResult(cardsToDiscard);
	}
}
