#nullable enable
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 强化遭遇已应用标记：挂在每个被强化的敌人身上，随战斗状态同步到所有客户端。
/// 用它判断"本场战斗是否已强化"，避免 static / 多端重复施放导致血量不一致；
/// BaseMaxHp 记录施放前的原始最大生命，供幂等计算（始终 base*1.5，重复应用结果一致）。
/// </summary>
[RegisterPower]
public class MonsterBoostedPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => false;

    // 施放强化前的原始最大生命（未放大），幂等计算用
    public decimal BaseMaxHp;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/BluntPower.png",
        BigIconPath: "res://images/powers/BluntPower.png"
    );
}