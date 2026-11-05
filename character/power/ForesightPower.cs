#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.Mini;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 看破：受到的伤害 +25%（固定，与易伤加算）。层数 = 剩余回合数，随其回合开始递减（与易伤一致）。
// 同时使其造成的伤害 -15%。
[RegisterPower]
public class ForesightPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/ForesightPower.png",
        BigIconPath: "res://images/powers/ForesightPower.png"
    );

    // 使其造成的伤害 -15%，受到伤害+25%
    // 注意：宿主是敌人等非玩家生物时 Owner.Player 为 null，不能依赖 Owner.Player?.Creature.Pets 遍历。
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay cardPlay)
    {
        // 白狼宠物造成的伤害 +50%
        if (target == Owner && dealer?.GetPowerAmount<WolfVanguard>() > 0)
        {
            return 1.5m;
        }

        // 看破目标受到的伤害 +25%
        if (target == Owner)
        {
            return 1.25m;
        }

        // 仅能力伤害受 -15% 减伤
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }

        // 非该生物自身造成的伤害不受减伤
        return dealer != Owner ? 1m : 0.85m;
    }

// 层数 = 剩余回合数：随宿主回合开始递减（与易伤一致）
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }
        await PowerCmd.Decrement(this);
    }
}