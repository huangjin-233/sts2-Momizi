#nullable enable
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;
[RegisterPower]
public class MilitaryMightPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/MilitaryMightPower.png",
        BigIconPath: "res://images/powers/MilitaryMightPower.png"
    );
    
}