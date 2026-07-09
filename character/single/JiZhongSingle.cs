using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace Momizi.character.single;

[RegisterSingleton]
public class JiZhongSingle: HookedSingletonModel
{
    // 填入HookType，可选Combat、Run或者None。Combat涉及战斗接口，Run涉及全局接口，具体看Hook类中的定义。
    public JiZhongSingle() : base(HookType.Run)
    {
    }
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack())
        {
            return 0m;
        }
        return dealer.GetPowerAmount<JiZhong>();
    }
}