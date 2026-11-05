#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Commands;
using Momizi.character.single;
using Momizi.RitsuAdapters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;


[RegisterPower]
public sealed class KiaiPoweUp : ModActionTemplate
{
    public override TargetType TargetType => TargetType.AnyEnemy;
    public override bool AutoRemoveAtTurnEnd => true;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override async Task OnAct(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        if (target == null || !target.IsAlive) return;
        IEnumerable<Creature> enumerable = from c in CombatState.GetTeammatesOf(Owner)
            where c.IsAlive && c.IsPlayer
            select c;
        foreach (Creature item in enumerable)
        {
            var actor = Owner;
            await MinionAnimCmd.PlayBumpAttackAsync(actor, item);
            await CreatureCmd.Damage(choiceContext, item, 4m, ValueProp.Move, actor);
            if (Owner.GetPowerAmount<Coordination>() >= 5)
            {
                await MinionAnimCmd.PlayBumpAttackAsync(actor, item);
                await CreatureCmd.Damage(choiceContext, actor,
                    WolfSingle.amount + Owner.Player.Creature.GetPowerAmount<StrengthPower>(), ValueProp.Move,
                    actor);
            }
            await PowerCmd.Remove<VigorPower>(actor);
            Coordination.firstattack = true;
            await PowerCmd.Remove<VigorPower>(Owner);
            await PowerCmd.Decrement(this);
        }
    }
}
