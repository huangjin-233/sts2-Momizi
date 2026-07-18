#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;
[RegisterPower]
public class Alert : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/Alert.png",
        BigIconPath: "res://images/powers/Alert.png"
    );

    public static int max = 20;

    public static bool inferior;
    protected override bool IsVisibleInternal
    {
        get
        {
            if (Owner.Player?.Character is Momizi)
            {
                return false;
            }
            return base.IsVisibleInternal;
        }
    }
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (Amount == 10 && power==this)
        {
            if (Owner.Player != null)
            {
                IEnumerable<Creature> enumerable = from c in CombatState.GetTeammatesOf(Owner.Player.Creature)
                    where c.IsAlive && c.IsPlayer && c != Owner
                    select c;
                foreach (Creature players in enumerable)
                {
                    await PowerCmd.Apply<StrengthPower>(choiceContext, players, 2, Owner, null);
                    await PowerCmd.Apply<DexterityPower>(choiceContext, players, 2, Owner, null);
                    await PowerCmd.Apply<JiZhong>(choiceContext, players, 2, Owner, null);
                }
                await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 2, Owner, null);
                await PowerCmd.Apply<Dodge>(choiceContext, Owner, 1, Owner, null);
                await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, 50, ValueProp.Unpowered, Owner);
            }
        }
        if (Amount == 20 && power==this && max !=20)
        {
            if (Owner.Player != null)
            {
                IEnumerable<Creature> enumerable = from c in CombatState.GetTeammatesOf(Owner.Player.Creature)
                    where c.IsAlive && c.IsPlayer && c != Owner
                    select c;
                foreach (Creature players in enumerable)
                {
                    await PowerCmd.Apply<StrengthPower>(choiceContext, players, 2, Owner, null);
                    await PowerCmd.Apply<DexterityPower>(choiceContext, players, 2, Owner, null);
                    await PowerCmd.Apply<JiZhong>(choiceContext, players, 2, Owner, null);
                }
                await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 2, Owner, null);
                await PowerCmd.Apply<Dodge>(choiceContext, Owner, 1, Owner, null);
                await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, 50, ValueProp.Unpowered, Owner);
            }
        }
        if (Amount == max && power==this)
        {
            inferior = true;
            await PowerCmd.Apply<Inferior>(choiceContext, Owner, 1, Owner, null);
            await PowerCmd.Remove(this);
            await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, 50, ValueProp.Unpowered, Owner);
        }
    }
    public override Task AfterCombatEnd(CombatRoom room)
    {
        max = 150;
        return Task.CompletedTask;
    }
}