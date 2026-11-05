#nullable enable
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 天狗的威光：本场战斗「大天狗」伤害 +Amount（Amount 由卡牌升级：6→10）。
// 由 Tengu.OnRightClick 读取 Amount 加成到伤害。
[RegisterPower]
public class TenguGloryPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/TenguGloryPower.png",
        BigIconPath: "res://images/powers/TenguGloryPower.png"
    );
}