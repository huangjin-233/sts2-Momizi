#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 赌注·蓄力：记录下回合开始时要给予的格挡量（Amount）。仅由「赌注」卡施加。
/// 玩家回合开始时发放格挡后自移。
/// </summary>
[RegisterPower]
public class WagerPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/WagerPower.png",
        BigIconPath: "res://images/powers/WagerPower.png"
    );

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player || !Owner.IsAlive)
        {
            return;
        }
        // 下回合开始：发放记录格挡量，随后移除本能力
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
        await PowerCmd.Remove(this);
    }
}