using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MinionLib.Commands;
using MinionLib.Minion;
using Momizi.character.Mini;
using Momizi.character.relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 白狼哨火：每回合开始自动召唤一条白狼（优先 WolfOne，上限 2 条）。
// 当两条白狼同时在场时，给两条白狼各 +2 力量。
[RegisterPower]
public class BeaconFirePower : ModPowerTemplate
{
    // 固定给白狼的力量值
    private const int PowerAmount = 2;

    public override PowerType Type => PowerType.Buff;
    // Single：该能力每战斗仅一层（是否触发只看白狼在场数，不随层数叠加）
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/BeaconFirePower.png",
        BigIconPath: "res://images/powers/BeaconFirePower.png"
    );

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 一次召唤一条：优先空缺的 WolfOne 槽位，其次 WolfTwo，两条已满则不再召唤（按该玩家自己 pets 判断）。
        decimal wolfMaxHp1 = Owner.Player.GetRelic<TenguSee>() is { } t1
            ? t1.DynamicVars.MaxHp.BaseValue
            : Owner.Player.GetRelic<MountainSee>() is { } m1
                ? m1.DynamicVars.MaxHp.BaseValue
                : DynamicVars.MaxHp.BaseValue;
        decimal wolfMaxHp2 = Owner.Player.GetRelic<TenguSee>() is { } t2
            ? t2.DynamicVars["Max"].BaseValue
            : Owner.Player.GetRelic<MountainSee>() is { } m2
                ? m2.DynamicVars["Max"].BaseValue
                : DynamicVars["Max"].BaseValue;
        // 白狼基础力量以遗物持久化的 baseStrength 为准（跨战斗保留）
        decimal wolfStrength1 = Owner.Player.GetRelic<TenguSee>() is { } s1
            ? s1.DynamicVars["Power1"].BaseValue
            : Owner.Player.GetRelic<MountainSee>() is { } ms1
                ? ms1.DynamicVars["Power1"].BaseValue
                : 0m;
        decimal wolfStrength2 = Owner.Player.GetRelic<TenguSee>() is { } s2
            ? s2.DynamicVars["Power2"].BaseValue
            : Owner.Player.GetRelic<MountainSee>() is { } ms2
                ? ms2.DynamicVars["Power2"].BaseValue
                : 0m;
        if (!WolfOne.OwnedBy(player))
        {
            await MinionCmd.AddMinion<WolfOne>(choiceContext, player, new MinionSummonOptions(
                MaxHp: wolfMaxHp1,
                PrimaryStatAmount: wolfStrength1,
                Source: null,
                Position: MinionPosition.Front));
        }
        else if (!WolfTwo.OwnedBy(player))
        {
            await MinionCmd.AddMinion<WolfTwo>(choiceContext, player, new MinionSummonOptions(
                MaxHp: wolfMaxHp2,
                PrimaryStatAmount: wolfStrength2,
                Source: null,
                Position: MinionPosition.Front));
        }

        // 两条白狼同时在场时，给该玩家自己的两条白狼各 +力量（白狼均带 WolfSlot 标记以资识别）
        if (WolfOne.OwnedBy(player) && WolfTwo.OwnedBy(player))
        {
            foreach (var pet in player.Creature.Pets)
            {
                if (pet.IsAlive && pet.HasPower<WolfSlot>())
                {
                    await PowerCmd.Apply<StrengthPower>(choiceContext, pet, PowerAmount, Owner, null);
                }
            }
        }
        await PowerCmd.Decrement(this);
    }
}