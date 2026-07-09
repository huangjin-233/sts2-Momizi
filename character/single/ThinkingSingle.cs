using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace Momizi.character.single;

[RegisterSingleton]
public class ThinkingSingle : HookedSingletonModel
{
    // 填入HookType，可选Combat、Run或者None。Combat涉及战斗接口，Run涉及全局接口，具体看Hook类中的定义。
    public ThinkingSingle() : base(HookType.Run)
    {
    }
    public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
     {
         if (card.Keywords.Contains(MomiziKeyword.Thinking))
         {
             await CardPileCmd.Draw(choiceContext, 1,card.Owner );//抽卡 
         }
     }
}