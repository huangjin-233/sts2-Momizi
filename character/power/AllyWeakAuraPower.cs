#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 强化遭遇词条「回合开始对全体友军施加 2 层虚弱」：挂在每个敌人身上，
/// 每回合开始时对玩家方生物（含宠物）施加 2 层虚弱。
/// </summary>
[RegisterPower]
public class AllyWeakAuraPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/Alert.png",
        BigIconPath: "res://images/powers/Alert.png"
    );

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player)
        {
            return;
        }
        var ctx = new ThrowingPlayerChoiceContext();
        foreach (var player in combatState.Players)
        {
            if (player.Creature == null || !player.Creature.IsAlive)
            {
                continue;
            }
            await PowerCmd.Apply<WeakPower>(ctx, player.Creature, 2, Owner, null);
            if (player.Creature.Pets != null)
            {
                foreach (var pet in player.Creature.Pets)
                {
                    if (pet.IsAlive)
                    {
                        await PowerCmd.Apply<WeakPower>(ctx, pet, 2, Owner, null);
                    }
                }
            }
        }
    }
}