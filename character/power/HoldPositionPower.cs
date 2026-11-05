#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 固守：每回合开始前，椛与全体友军随从各获得格挡（升级提升格挡量）
[RegisterPower]
public class HoldPositionPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/HoldPositionPower.png",
        BigIconPath: "res://images/powers/HoldPositionPower.png"
    );

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            // 回合开始前，全体友军（椛及所有随从）各获得格挡
                await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
            foreach (var player in Owner.Player?.Creature.CombatState?.Allies!)
            {
                if (player.IsAlive)
                {
                    await CreatureCmd.GainBlock(player, Amount, ValueProp.Unpowered, null);
                }
            }
        }
    }
}