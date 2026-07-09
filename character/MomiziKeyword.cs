using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Momizi.character
{
	[RegisterOwnedCardKeyword(nameof(Loyalty))]
	[RegisterOwnedCardKeyword(nameof(SelfEsteem))]
	[RegisterOwnedCardKeyword(nameof(Collaboration))]
	[RegisterOwnedCardKeyword(nameof(Thinking),CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
	[RegisterOwnedCardKeyword(nameof(Spell),CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
	[RegisterOwnedCardKeyword(nameof(Debuff))]
	internal class MomiziKeyword
	{
		public static readonly CardKeyword  Loyalty = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Loyalty)).GetModCardKeyword();
		public static readonly CardKeyword  SelfEsteem = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(SelfEsteem)).GetModCardKeyword();
		public static readonly CardKeyword  Collaboration = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Collaboration)).GetModCardKeyword();
		public static readonly CardKeyword  Thinking = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Thinking)).GetModCardKeyword();
		public static readonly CardKeyword  Spell = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Spell)).GetModCardKeyword();
		public static readonly CardKeyword  Debuff = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Debuff)).GetModCardKeyword();
	}
}
