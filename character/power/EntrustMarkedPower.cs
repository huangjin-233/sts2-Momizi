#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 交托·被瞄准：由「交托」卡施加在目标白狼上，仅对该卡生效。
/// 效果：把原本指向椛本体或另一只白狼的未格挡伤害，改为打到持有本 Power 的这只狼（嘲讽/护主）。
/// 战斗结束或宿主死亡时移除。
/// </summary>
[RegisterPower]
public class EntrustMarkedPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/AimedAt.png",
        BigIconPath: "res://images/powers/AimedAt.png"
    );

    // 把指向椛本体或另一只白狼的未格挡伤害，重定向到该狼自身。
    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        if (!Owner.IsAlive)
        {
            return target;
        }
        // 主人的生物（椛）；白狼的 Owner.Player 为 null，须用 Owner.PetOwner。
        Creature? masterCreature = Owner.PetOwner?.Creature;
        // 目标就是本狼：无需重定向。
        if (target == Owner)
        {
            return target;
        }
        // 只拦截「椛本体」或「另一只白狼随从」两个目标的伤害；敌方等其他目标不吸。
        if (target != masterCreature && target.GetPowerAmount<WolfVanguard>() <= 0)
        {
            return target;
        }
        return Owner;
    }

    // 回合结束移除，保证仅对当次打出生效（与交托"本回合"语义一致）。
    public override async Task AfterEnergyReset(Player player)
    {
        if (player == Owner.PetOwner)
        {
            await PowerCmd.Remove(this);
        }
    }
}