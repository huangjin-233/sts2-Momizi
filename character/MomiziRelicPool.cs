#nullable enable
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character;

public class MomiziRelicPool: TypeListRelicPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://images/ui/energys/momizi_energy_icon.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://images/ui/energys/momizi_energy_icon_big.png";

    public override string EnergyColorName => "Momizi";
}