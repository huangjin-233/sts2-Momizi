#nullable enable
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 预命中标记：显示某生物即将被敌方单体攻击命中（受击前显示）。
/// 由 WolfVanguard 在回合开始时决定并应用，命中后于下个回合开始时清除/刷新。
/// </summary>
[RegisterPower]
public class AimedAt : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/AimedAt.png",
        BigIconPath: "res://images/powers/AimedAt.png"
    );
}