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
using STS2RitsuLib.Scaffolding.Content;
#pragma warning disable CS8602 // 解引用可能出现空引用。

namespace Momizi.character.power;

[RegisterPower]
public class JiZhong: ModPowerTemplate
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/JiZhong.png",
        BigIconPath: "res://images/powers/JiZhong.png"
    );
    
    public override bool AllowNegative => true;

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource,
        CardPlay? cardPlay)
    {
            if (cardSource != null)
            {
                if (cardSource.Owner.Creature != Owner)
                {
                    return 0m;
                }
            }
            else if (Owner != target)
            {
                return 0m;
            }

            if (!props.IsPoweredCardOrMonsterMoveBlock())
            {
                return 0m;
            }

            return Amount/2;
    }
    
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel powers, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (powers == this)
        {
            if (Owner.Player?.Character is not Momizi)
            {
                await PowerCmd.Apply<FocusPower>(choiceContext, Owner, amount, Owner, null);
                await PowerCmd.Remove(this);
            }
        }
    }
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource)
    {
        if ( Owner!= dealer)
        {
            return 0m;
        }
        if (!props.IsPoweredAttack())
        {
            return 0m;
        }
        if (cardSource == null)
        {
            return 0m;
        }
        if (cardSource.Keywords.Contains(MomiziKeyword.Spell))
        {
            return dealer.GetPowerAmount<JiZhong>()/2-dealer.GetPowerAmount<StrengthPower>();
        }
        return dealer.GetPowerAmount<JiZhong>()/2;
    }
}