using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.uncommon;
using Momizi.character.power;
using Momizi.character.Scry;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace Momizi.character.single;

[RegisterSingleton]
public class WolfAttackSingle: HookedSingletonModel
{
    // 填入HookType，可选Combat、Run或者None。Combat涉及战斗接口，Run涉及全局接口，具体看Hook类中的定义。
    public WolfAttackSingle() : base(HookType.Run)
    {
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props,
        Creature target, CardModel cardSource)
    {
        if (cardSource != null && dealer == cardSource.Owner.Creature && !target.IsAlive)
            Coordination.firstattack = false;
    }
}