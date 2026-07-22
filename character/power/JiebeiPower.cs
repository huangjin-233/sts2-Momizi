#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class JiebeiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/JiebeiPower.png",
        BigIconPath: "res://images/powers/JiebeiPower.png"
    );

    private bool threatenedThisTurn;

    private static bool IsHarmful(AbstractIntent intent)
    {
        return intent.IntentType is IntentType.Attack or IntentType.DeathBlow
            or IntentType.Debuff or IntentType.DebuffStrong
            or IntentType.CardDebuff or IntentType.StatusCard;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy)
        {
            return Task.CompletedTask;
        }

        threatenedThisTurn = participants.Any(c => c.IsAlive && c.Monster != null
            && c.Monster.NextMove.Intents.Any(IsHarmful));
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy)
        {
            return;
        }

        var owner = Owner.PetOwner?.Creature;
        if (threatenedThisTurn && owner is { IsAlive: true })
        {
            threatenedThisTurn = false;
            // 「戒备」状态附着在白狼；其产出的警戒仍是玩家资源。
            await PowerCmd.Apply<Alert>(choiceContext, owner, Amount, Owner, null);
        }
    }
}
