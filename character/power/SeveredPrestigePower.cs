#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class SeveredPrestigePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/SeveredPrestigePower.png",
        BigIconPath: "res://images/powers/SeveredPrestigePower.png"
    );

    private bool first = true;
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner && props.IsPoweredAttack() && result.UnblockedDamage > 0 && first)
        {
            await PowerCmd.Apply<ForesightPower>(choiceContext, target, 1, Owner, null);
            first = false;
        }
        foreach (var pet in Owner.Player?.Creature.Pets!)
        {
            if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
            {
                if (dealer == pet)
                    await PowerCmd.Apply<WeakPower>(choiceContext, target, 1, Owner, null); // 白狼攻击：施加1层虚弱
            }
        }
    }

    // 白狼对带看破的目标额外造成 +Amount（Amount 由卡牌传入：5）点伤害
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer == null || target == null || !props.IsPoweredAttack())
        {
            return 0m;
        }
        if (dealer.GetPowerAmount<WolfVanguard>() <= 0)
        {
            return 0m;
        }
        if (target.GetPowerAmount<ForesightPower>() <= 0)
        {
            return 0m;
        }
        return Amount;
    }
}