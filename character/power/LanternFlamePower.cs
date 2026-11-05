#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 长明哨火：每回合开始时，若警戒 ≥10，获得 1 力量与 Amount 点格挡（Amount 由卡牌升级：4→6）。
[RegisterPower]
public class LanternFlamePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/LanternFlamePower.png",
        BigIconPath: "res://images/powers/LanternFlamePower.png"
    );

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }
        if (Owner.GetPowerAmount<Alert>() < 10)
        {
            return;
        }
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}