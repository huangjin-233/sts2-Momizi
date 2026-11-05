#nullable enable
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 内部标识 power：仅标记该白狼属于哪个槽位（Amount==1 为 WolfOne/一号槽，==2 为 WolfTwo/二号槽）。
/// 用于按玩家遍历场上 pets 判断"该玩家是否已有某只狼"，替代多人下不可靠的全局 static 布尔。
/// 应用后由 Wolf.OnSummon 将 IsVisible 置 false，不显示在力量栏及图鉴，不参与任何战斗逻辑。
/// </summary>
[RegisterPower]
public class WolfSlot : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // Counter：以 Amount 存储槽位 id（1/2）
    public override PowerStackType StackType => PowerStackType.Counter;

    // 纯内部标识，不在力量栏/图鉴显示
    protected override bool IsVisibleInternal => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/Alert.png",
        BigIconPath: "res://images/powers/Alert.png"
    );
}