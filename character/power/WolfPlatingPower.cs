#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 白狼专属覆甲：原版 PlatingPower 在 BeforeSideTurnEndEarly 中依赖
/// participants.Contains(Owner) 才结算格挡，召唤物（白狼）不在参与者列表中，
/// 导致覆甲永不获得格挡。本 power 在己方回合结束前直接给白狼格挡，不依赖参与者判断。
/// </summary>
[RegisterPower]
public sealed class WolfPlatingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !Owner.IsAlive)
        {
            return;
        }
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
        await PowerCmd.Decrement(this);
    }
}
