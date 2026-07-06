using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class Dodge: ModPowerTemplate
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Test/images/powers/Dodge.png",
        BigIconPath: "res://Test/images/powers/Dodge.png"
    );

    public static bool Active;
    // 抽牌后给予玩家力量
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel powers, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (powers.Type == PowerType.Debuff && applier.IsEnemy)
        {
            await PowerCmd.Remove(powers);
            await PowerCmd.Decrement(this);
            Active = true;
        }
    }
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner)
        {
            return 1m;
        }
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }
        return 0m;
    }
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult _, ValueProp props, Creature? dealer, CardModel? __)
    {
        if (target == base.Owner && dealer != null && props.IsPoweredAttack())
        {
            await PowerCmd.Decrement(this);
        }
    }
    public override async Task AfterEnergyReset(Player player)
    {
        await PowerCmd.Remove(this);
    }
}