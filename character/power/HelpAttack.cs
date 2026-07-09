using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
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
public class HelpAttack: ModPowerTemplate
{
    private class Data
    {
        public AttackCommand? commandToModify;

        public int amountWhenAttackStarted;
    }
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Test/images/powers/Inferior.png",
        BigIconPath: "res://Test/images/powers/Inferior.png"
    );

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.Attacker != base.Owner)
        {
            return Task.CompletedTask;
        }
        if (!command.DamageProps.IsPoweredAttack())
        {
            return Task.CompletedTask;
        }
        Data internalData = GetInternalData<Data>();
        if (internalData.commandToModify != null)
        {
            return Task.CompletedTask;
        }
        if (command.ModelSource != null && !(command.ModelSource is CardModel))
        {
            return Task.CompletedTask;
        }
        if (!command.DamageProps.IsPoweredAttack())
        {
            return Task.CompletedTask;
        }
        internalData.commandToModify = command;
        internalData.amountWhenAttackStarted = base.Amount;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (Owner != dealer)
        {
            return 0m;
        }
        if (!props.IsPoweredAttack())
        {
            return 0m;
        }
        Data internalData = GetInternalData<Data>();
        if (internalData.commandToModify != null && cardSource != null && cardSource != internalData.commandToModify.ModelSource)
        {
            return 0m;
        }
        if (internalData.commandToModify != null && internalData.commandToModify.Attacker != dealer)
        {
            return 0m;
        }
        return Amount;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        Data internalData = GetInternalData<Data>();
        if (command == internalData.commandToModify)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -internalData.amountWhenAttackStarted, null, null);
        }
    }
}