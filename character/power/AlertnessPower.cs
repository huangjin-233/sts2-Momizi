#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 警觉：每回合结束时，获得 Amount 层警戒
[RegisterPower]
public class AlertnessPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // Counter：可叠加（升级后每回合2层）
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/AlertnessPower.png",
        BigIconPath: "res://images/powers/AlertnessPower.png"
    );

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        // 玩家回合结束时结算
        if (side != Owner.Side)
        {
            return;
        }
        await PowerCmd.Apply<Alert>(choiceContext, Owner, Amount, Owner, null);
    }
}