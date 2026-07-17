// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Watcher, Version=1.5.2.0, Culture=neutral, PublicKeyToken=null
// Watcher.Code.Events.IModifyScryAmount

using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Momizi.character.Scry;

public interface IModifyScryAmount
{
	int ModifyScryAmount(Player player, int amount);

	Task AfterModifyingScryAmount(PlayerChoiceContext ctx, Player player, int originalAmount, int modifiedAmount);
}