#nullable enable
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.special;
using Momizi.character.card.uncommon;
using Momizi.character.Scry;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class KeepWatchPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/KeepWatchPower.png",
        BigIconPath: "res://images/powers/KeepWatchPower.png"
    );

    // 护主：把指向玩家本体或另一只白狼的未格挡伤害，改为打到被指定的这只狼
    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        if (!Owner.IsAlive)
        {
            return target;
        }
        // 白狼随从的 Owner.Player 为 null，需用 Owner.PetOwner 取主人的生物（椛）
        var playerCreature = Owner.PetOwner?.Creature;
        if (target != Owner && (target == playerCreature || target.GetPowerAmount<WolfVanguard>() > 0) && target == KeepWatchPlayer.player)
        {
            return Owner;
        }
        return target;
    }
}