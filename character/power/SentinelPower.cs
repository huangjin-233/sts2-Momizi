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

// 守望者：每当你因「掩护」承受伤害，白狼获得2点力量 —— 模板，待实现
[RegisterPower]
public class SentinelPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/SentinelPower.png",
        BigIconPath: "res://images/powers/SentinelPower.png"
    );

    // 椛因「掩护」承受伤害：某只白狼开掩护(YanhuPower)时，椛承受了本应由狼承受的未格挡伤害。
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props,
        Creature target, CardModel? cardSource)
    {
        // 仅当椛自身受到实际未格挡伤害时判定
        if (target != Owner)
        {
            return;
        }
        // 椛本体没有直接受击来源时的转移伤害：寻找开启「掩护」的存活白狼
        foreach (var pet in Owner.Player?.Creature.Pets!)
        {
            if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0 && pet.HasPower<YanhuPower>())
            {
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), pet, Amount, Owner, null);
            }
        }
    }
}