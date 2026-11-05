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

/// <summary>
/// 变化：挂在敌人身上的 2 回合减益。受到未格挡伤害时，削减等量临时力量（施加"变化"临时力量）。
/// 由山窝打出时施加到目标敌人身上，按敌人回合计时。
/// </summary>
[RegisterPower]
public class ChangedPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    // 叠加类型，Counter表示可叠加（Amount 即剩余回合数）
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/ChangedPower.png",
        BigIconPath: "res://images/powers/ChangedPower.png"
    );

    // 敌人回合开始：计时 -1，自然衰减到 0 后移除（两回合)
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Enemy)
            await PowerCmd.Decrement(this);
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        // 受到未格挡伤害时，削减等量临时力量（变化）
        if (target == Owner && result.UnblockedDamage > 0)
            await PowerCmd.Apply<Change>(choiceContext, Owner, result.UnblockedDamage, Owner, null);
    }
}