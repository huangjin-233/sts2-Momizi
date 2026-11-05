using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Momizi.character
{
	[RegisterOwnedCardKeyword(nameof(Loyalty))]
	[RegisterOwnedCardKeyword(nameof(Collaboration))]
	[RegisterOwnedCardKeyword(nameof(Thinking),CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
	[RegisterOwnedCardKeyword(nameof(Spell),CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
	[RegisterOwnedCardKeyword(nameof(Debuff))]
	[RegisterOwnedCardKeyword(nameof(Scry))]
	[RegisterOwnedCardKeyword(nameof(Lock))]
	[RegisterOwnedCardKeyword(nameof(Rearrange))]
	[RegisterOwnedCardKeyword(nameof(Intelligence))]
	[RegisterOwnedCardKeyword(nameof(WhiteWolf))]
	[RegisterOwnedCardKeyword(nameof(Coordinations))]
	internal class MomiziKeyword
	{
		public static readonly CardKeyword  Loyalty = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Loyalty)).GetModCardKeyword();
		public static readonly CardKeyword  Collaboration = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Collaboration)).GetModCardKeyword();
		public static readonly CardKeyword  Thinking = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Thinking)).GetModCardKeyword();
		public static readonly CardKeyword  Spell = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Spell)).GetModCardKeyword();
		public static readonly CardKeyword  Debuff = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Debuff)).GetModCardKeyword();
		public static readonly CardKeyword  Scry = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Scry)).GetModCardKeyword();
		public static readonly CardKeyword  Lock = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Lock)).GetModCardKeyword();
		public static readonly CardKeyword  Rearrange = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Rearrange)).GetModCardKeyword();
		public static readonly CardKeyword  Intelligence = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Intelligence)).GetModCardKeyword();
		public static readonly CardKeyword  WhiteWolf = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(WhiteWolf)).GetModCardKeyword();
		public static readonly CardKeyword  Coordinations = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Coordinations)).GetModCardKeyword();
	}
}
