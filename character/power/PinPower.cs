#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class PinPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/PinPower.png",
        BigIconPath: "res://images/powers/PinPower.png"
    );

    // 掩护：椛与这只被牵制的白狼互相掩护对方的未格挡伤害——
    // 狼掩护你：指向椛本体或另一只白狼的伤害，改为由本狼承担；
    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        if (!Owner.IsAlive)
        {
            return target;
        }
        // 白狼随从的 Owner.Player 为 null，需用 Owner.PetOwner 取主人的生物（椛）。
        Creature? master = Owner.PetOwner?.Creature;
        // 狼掩护你：椛本体，或与本狼同属一个主人（同一玩家）的另一只白狼受到的伤害 → 由本狼承担。
        // 用 PetOwner 判定同队，避免本狼误挡队友（另一玩家）狼的伤害。
        if (target == master
            || (target.PetOwner == Owner.PetOwner
                && target != Owner
                && target.GetPowerAmount<WolfVanguard>() > 0))
        {
            return Owner;
        }
        return target;
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (player == Owner.PetOwner)
        {
            await PowerCmd.Remove(this);
        }
    }
}