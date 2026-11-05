#nullable enable
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 天狗的认可：召唤「大天狗」时你也获得 Amount 点力量与敏捷（Amount 由卡牌升级：2→3）。
// 由 Tengu.OnRightClick 读取，在大天狗发动时同步给椛力量/敏捷。
[RegisterPower]
public class TenguApprovalPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/TenguApprovalPower.png",
        BigIconPath: "res://images/powers/TenguApprovalPower.png"
    );
}