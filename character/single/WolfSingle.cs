#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Commands;
using Momizi.character.card.basic;
using Momizi.character.card.uncommon;
using Momizi.character.power;
using Momizi.character.relics;
using Momizi.character.Scry;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Models;
#pragma warning disable CS8602 // 解引用可能出现空引用。

namespace Momizi.character.single;

[RegisterSingleton]
public class WolfSingle: HookedSingletonModel
{
    // 填入HookType，可选Combat、Run或者None。Combat涉及战斗接口，Run涉及全局接口，具体看Hook类中的定义。
    public WolfSingle() : base(HookType.Combat)
    {
    }

    public static decimal amount;
    // 每位玩家每回合仅触发一次狼的跟随攻击：按打出攻击牌的玩家各自计数，
    // 避免队友的攻击牌（同一战斗单例共享状态）抢占本玩家的触发资格。
    private readonly HashSet<Player> _wolfAttacked = new();
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }
        if (cardPlay.Target == null || !cardPlay.Target.IsAlive)
        {
            return;
        }
        var owner = cardPlay.Card.Owner;
        // 该玩家本回合已跟随攻击过
        if (_wolfAttacked.Contains(owner))
        {
            return;
        }
        // 白狼的额外攻击伤害 = 实际拥有的遗物（天狗的见识/山的见识）Damage 基础值
        if (owner.GetRelic<TenguSee>() is { } tengu)
        {
            amount = tengu.DynamicVars.Damage.BaseValue;
        }
        else if (owner.GetRelic<MountainSee>() is { } mountain)
        {
            amount = mountain.DynamicVars.Damage.BaseValue;
        }
        else
        {
            return; // 无相关遗物，不追加白狼攻击
        }
        _wolfAttacked.Add(owner);
        await Task.Delay(350);
        // 每只在场且存活的白狼都会对同一目标进行一次攻击。
        if (cardPlay.Target != null && cardPlay.Target.IsAlive)
        {
            bool attacked = false;
            foreach (var pet in owner.Creature.Pets)
            {
                if (!cardPlay.Target.IsAlive)
                {
                    break;
                }

                if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
                {
                    await MinionAnimCmd.PlayBumpAttackAsync(pet, cardPlay.Target);
                    await CreatureCmd.Damage(choiceContext, cardPlay.Target,
                        amount + owner.Creature.GetPowerAmount<StrengthPower>(), ValueProp.Move, pet);
                    attacked = true;
                    await Task.Delay(150);
                    await PowerCmd.Remove<VigorPower>(pet);
                    Coordination.firstattack = true;
                }
            }
            if (!attacked)
            {
                _wolfAttacked.Remove(owner);
            }
            await PowerCmd.Remove<VigorPower>(owner.Creature);
        }
        else
        {
            _wolfAttacked.Remove(owner);
        }
    }
    //代替承受：同调+1
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            foreach (var pet in participants)
            {
                if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0)
                {
                    if (pet.HasPower<YanhuPower>())
                    {
                        await PowerCmd.Apply<Coordination>(choiceContext, pet, 1, pet, null);
                    }
                }
            }
        }
    }

    public override async Task AfterEnergyReset(Player player)
    {
        // 该玩家自己的回合重置时，清掉自己的跟随攻击计数
        _wolfAttacked.Remove(player);
        await Task.CompletedTask;
        Coordination.firstattack = false;
        Coordination.firstblock = true;
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        Coordination.firstblock = true;
        Coordination.firstattack = false;
    }
}
