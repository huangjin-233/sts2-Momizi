// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Watcher, Version=1.5.2.0, Culture=neutral, PublicKeyToken=null
// Watcher.Code.Events.ScryHook
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

public class ScryHook
{
	private static async Task Dispatch<T>(PlayerChoiceContext ctx, Player player, Func<T, Task> invoke) where T : class
	{
		ICombatState combatState = player.Creature.CombatState;
		if (combatState == null)
		{
			return;
		}
		foreach (T item in combatState.IterateHookListeners().OfType<T>())
		{
			AbstractModel abstractModel = (AbstractModel)(object)item;
			ctx.PushModel(abstractModel);
			await invoke(item);
			ctx.PopModel(abstractModel);
		}
	}

	private static TResult Aggregate<T, TResult>(ICombatState combatState, TResult seed, Func<T, TResult, TResult> action) where T : class
	{
		return combatState.IterateHookListeners().OfType<T>().Aggregate(seed, (TResult current, T model) => action(model, current));
	}

	public static TValue Modify<THook, TValue>(ICombatState? combatState, TValue originalAmount, Func<THook, TValue, TValue> amountModifier, out IEnumerable<THook> modifiers) where THook : class where TValue : IEquatable<TValue>
	{
		if (combatState == null)
		{
			modifiers = Array.Empty<THook>();
			return originalAmount;
		}
		TValue val = originalAmount;
		List<THook> list = new List<THook>();
		foreach (THook item in combatState.IterateHookListeners().OfType<THook>())
		{
			TValue val2 = val;
			val = amountModifier(item, val);
			if (!val2.Equals(val))
			{
				list.Add(item);
			}
		}
		modifiers = list;
		return val;
	}

	public static async Task AfterModifying<THook>(ICombatState cs, IEnumerable<THook> modifiers, Func<THook, Task> action) where THook : class
	{
		HashSet<THook> modifierSet = new HashSet<THook>(modifiers);
		foreach (THook iterateHookListener in cs.IterateHookListeners().OfType<THook>())
		{
			if (modifierSet.Contains(iterateHookListener))
			{
				await action(iterateHookListener);
				if (iterateHookListener is AbstractModel abstractModel)
				{
					abstractModel.InvokeExecutionFinished();
				}
			}
		}
	}
	

	public static Task AfterScryed(PlayerChoiceContext ctx, Player player, int scryAmount, int discardedAmount, IEnumerable<CardModel> discarded)
	{
		return Dispatch(ctx, player, (IAfterScryed m) => m.AfterScryed(ctx, player, scryAmount, discardedAmount, discarded));
	}
	public static int ModifyScryAmount(Player player, int amount, out IEnumerable<IModifyScryAmount> modifiers)
	{
		return Modify(player.Creature.CombatState, amount, (IModifyScryAmount m, int a) => m.ModifyScryAmount(player, a), out modifiers);
	}

	public static Task AfterModifyingScryAmount(PlayerChoiceContext ctx, Player player, IEnumerable<IModifyScryAmount> modifiers, int originalAmount, int modifiedAmount)
	{
		return AfterModifying(player.Creature.CombatState, modifiers, (IModifyScryAmount a) => a.AfterModifyingScryAmount(ctx, player, originalAmount, modifiedAmount));
	}
}
