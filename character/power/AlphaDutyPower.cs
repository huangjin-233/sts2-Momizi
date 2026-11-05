#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;
[RegisterPower]
public class AlphaDutyPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/AlphaDutyPower.png",
        BigIconPath: "res://images/powers/AlphaDutyPower.png"
    );

    private int damaged = 0;
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner )
        {
            damaged += result.TotalDamage;
            while (damaged >= 20)
            {
                damaged -= 20;
                await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 3*Amount, Owner, null);
                await PowerCmd.Apply<ReduceDamge>(choiceContext, Owner, 1, Owner, null);
            }
        }
    }
}