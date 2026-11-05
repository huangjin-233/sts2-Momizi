#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.uncommon;
using Momizi.character.power;
using Momizi.character.Scry;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
#pragma warning disable CS8602 // 解引用可能出现空引用。

namespace Momizi.character.single;

[RegisterSingleton]
public class JiZhongSingle: HookedSingletonModel
{
    // 填入HookType，可选Combat、Run或者None。Combat涉及战斗接口，Run涉及全局接口，具体看Hook类中的定义。
    public JiZhongSingle() : base(HookType.Run)
    {
    }
   
}