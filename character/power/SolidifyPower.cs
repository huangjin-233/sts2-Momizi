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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 巩固：你的格挡在回合开始时不再被清除 —— 模板，待实现
// 注意：格挡在回合开始被清除是游戏内建逻辑，本能力可能需要通过 Transpiler 改写或专门 hook。
[RegisterPower]
public class SolidifyPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/SolidifyPower.png",
        BigIconPath: "res://images/powers/SolidifyPower.png"
    );

    private int blocknext;

    public override bool ShouldClearBlock(Creature creature)
    {
        return creature != Owner;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Enemy)
        {
            return;
        }

        blocknext = 0;
        foreach (var enemy in combatState.Enemies.ToList())
        {
            if (!enemy.IsAlive || enemy.Monster?.NextMove == null)
            {
                continue;
            }

            foreach (var intent in enemy.Monster.NextMove.Intents.OfType<AttackIntent>())
            {
                blocknext += (int)(intent.DamageCalc?.Invoke() ?? 0m);
            }
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        int retainedBlockLoss = Owner.Block / 2;
        if (retainedBlockLoss > 0)
        {
            await CreatureCmd.LoseBlock(choiceContext,Owner, retainedBlockLoss,Owner);
        }
        await CreatureCmd.GainBlock(Owner, blocknext, ValueProp.Unpowered, null);
        blocknext = 0;
    }
}
