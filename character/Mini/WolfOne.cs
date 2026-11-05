#nullable enable
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MinionLib.Minion;
using MinionLib.Powers;
using Momizi.character.card.basic;
using Momizi.character.card.special;
using Momizi.character.power;

namespace Momizi.character.Mini;

public sealed class WolfOne : MinionModel
{
    public static bool wolf1 = false;
    // 运行时当前在场实例，仅用于卡牌目标识别（不持久化）
    public static WolfOne? Current = null;
    public override int MinInitialHp => 10;
    public override int MaxInitialHp => 14;
    protected override string VisualsPath =>
        "res://Example/MinionTest/scenes/creature_visuals/WolfOne.tscn";

    // 该玩家当前是否拥有 1 号槽位的白狼（WolfOne）。按玩家自身 pets 判断，替代全局 static。
    public static bool OwnedBy(Player owner)
        => owner.Creature != null && owner.Creature.Pets.Any(p => p.IsAlive && p.GetPowerAmount<WolfSlot>() == 1);

    public override async Task OnSummon(
        PlayerChoiceContext choiceContext,
        Player owner,
        MinionSummonOptions options)
    {
        Current = this;
        var self = Creature;

        if (options.MaxHp is decimal maxHp)
            await CreatureCmd.SetMaxAndCurrentHp(self, maxHp);

        if (options.PrimaryStatAmount is decimal strength && strength > 0m)
            await PowerCmd.Apply<StrengthPower>(
                choiceContext, self, strength, owner.Creature, options.Source);

        // 护主：敌方攻击椛时，未格挡伤害 50% 概率改由白狼承受
        await PowerCmd.Apply<WolfVanguard>(
            choiceContext, self, 1, owner.Creature, options.Source);

        // 槽位标记：标记这只狼为 1 号槽（WolfOne），供按玩家查询槽位占用（多人下不依赖全局 static）。
        await PowerCmd.Apply<WolfSlot>(choiceContext, self, 1, owner.Creature, options.Source);
    }

    // 白狼死亡：你获得 1 层自卑
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature dead,
        bool wasDead,
        float damageSource)
    {
        if (dead.PetOwner is { } owner && owner.Creature != null)
        {
            if (dead.PetOwner.Creature.HasPower<LastWillPower>())
            {
                var strength = dead.GetPowerAmount<Coordination>() * dead.PetOwner.Creature.GetPowerAmount<LastWillPower>() ;
                await PowerCmd.Apply<StrengthPower>(choiceContext, dead.PetOwner.Creature, strength, dead, null);
            }
            await PowerCmd.Apply<Inferior>(choiceContext, owner.Creature, 1, owner.Creature, null);
            var command = WhiteWolfGuard.coomd1;
            WhiteWolfGuard.coomd1 = null;
            // 用 RemoveFromCombat 将指令卡移出战斗（不触发战斗钩子迭代，可安全重复调用；
            // 卡已被消耗/离场时亦不会空引用）。避免在死亡回调中 Exhaust 引发监听器迭代崩溃。
            if (command != null && command.Pile != null && command.Pile.Type != PileType.Exhaust)
            {
                await CardPileCmd.RemoveFromCombat(command, false);
            }
        }
    }
}
