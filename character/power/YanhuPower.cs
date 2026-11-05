#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class YanhuPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/YanhuPower.png",
        BigIconPath: "res://images/powers/YanhuPower.png"
    );

    // 白狼受到的任何伤害，改由其主人承受；经过主人的格挡。

    // 升级后：椛每回合获得 Amount 点格挡。
    public override async Task AfterEnergyReset(Player player)
    {
        var owner = Owner.PetOwner?.Creature;
        if (player == Owner.PetOwner && owner is { IsAlive: true } && Amount > 0)
        {
            await CreatureCmd.GainBlock(owner, Amount, ValueProp.Move, null);
        }
    }
}
