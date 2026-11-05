#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 敏锐直觉：每回合你首次获得警戒时，该次获得的层数翻倍。
// 具体的翻倍逻辑由 Alert.AfterPowerAmountChanged 检查本状态的 UsedThisTurn 完成。
[RegisterPower]
public class KeenInsightPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // Single：效果只存在一次，不叠加
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/KeenInsightPower.png",
        BigIconPath: "res://images/powers/KeenInsightPower.png"
    );

    // 本回合是否已触发过翻倍
    public bool UsedThisTurn;

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        // 玩家回合开始重置
        if (side == Owner.Side)
        {
            UsedThisTurn = false;
        }
        return Task.CompletedTask;
    }
}