using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;
using Momizi;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Teamy.character
{
	[RegisterOwnedCardKeyword(nameof(Loyalty))]
	[RegisterOwnedCardKeyword(nameof(SelfEsteem))]
	[RegisterOwnedCardKeyword(nameof(Collaboration))]
	internal class MomiziKeyword
	{
		public static readonly CardKeyword  Loyalty = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Loyalty)).GetModCardKeyword();
		public static readonly CardKeyword  SelfEsteem = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(SelfEsteem)).GetModCardKeyword();
		public static readonly CardKeyword  Collaboration = ModContentRegistry.GetQualifiedKeywordId(ModInitializer.ModId, nameof(Collaboration)).GetModCardKeyword();
	}
}
