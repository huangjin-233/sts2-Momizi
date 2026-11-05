#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
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
using Momizi.character.rewards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
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

    // 警戒上限 20（场景覆盖层 Alerts.cs 也引用该字段，需从所在生物取得实例）
    public int max = 20;
    public bool IsClampingToMax;

    // 本回合获得过的警戒层数（每回合开始清零；供反击/敏锐等查询）
    public int GainedThisTurn;
    public int alert;
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

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player) 
            GainedThisTurn = 0;
    }

    // 警戒不衰减、不触发爆炸；仅做上限钳制。同时处理「敏锐直觉」翻倍。
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this)
        {
            return;
        }

        // 敏锐直觉：每回合首次获得警戒时翻倍
        if (Amount > 0 && amount > 0)
        {
            var keen = Owner.FindPower<KeenInsightPower>();
            if (keen != null && !keen.UsedThisTurn)
            {
                keen.UsedThisTurn = true;
                await PowerCmd.ModifyAmount(choiceContext, this, amount, applier, cardSource);
            }
            GainedThisTurn += (int)amount;
            alert += (int)amount;
        }

        // 上限钳制：超出的部分扣回至上限；溢出按「绷紧的弓弦」转化为临时力量
        if (Amount > max)
        {
            int overflow = Amount - max;
            foreach (var pet in Owner.Player?.Creature.Pets!)
            {
                if (pet.IsAlive && pet.GetPowerAmount<WolfVanguard>() > 0 && pet.HasPower<JointHartredPower>())
                {
                    int blockandvigor = overflow * pet.GetPowerAmount<JointHartredPower>();
                    await CreatureCmd.GainBlock(pet, blockandvigor,ValueProp.Move, null);//防御
                    await PowerCmd.Apply<VigorPower>(choiceContext, pet, blockandvigor, Owner, null);
                }
            }
            IsClampingToMax = true;
            try
            {
                Amount = max;
            }
            finally
            {
                IsClampingToMax = false;
            }
            var bow = Owner.GetPowerAmount<TightBowstringPower>();
            if (Owner.HasPower<TightBowstringPower>())
            {
                int strength = overflow / 2;
                if (strength > 0)
                {
                    await PowerCmd.Apply<TightBowstringPowers>(choiceContext, Owner, bow * strength, Owner, null);
                }
            }
            if (Owner.HasPower<TightBowstringPowered>())
            {
                int strength = overflow;
                if (strength > 0)
                {
                    await PowerCmd.Apply<TightBowstringPowers>(choiceContext, Owner, bow * strength, Owner, null);
                }
            }
            var enenrgy = Owner.GetPowerAmount<OverflowPower>();
            if (Owner.HasPower<OverflowPower>())
            {
                int over = overflow;
                if (over > 0)
                {
                    await PowerCmd.Apply<TightBowstringPowers>(choiceContext, Owner, enenrgy, Owner, null);
                }
            }
        }
    }
    

    // 战斗结束：按档位转化为情报（≥15→3、≥10→2、≥5→1；25% 额外 +1，精英战 50%）。
    // 情报只通过奖励磁贴发放（官方奖励选取流程会在多端复制，保证各客户端同步），不再直接 AddIntelligence。
    // 额外 +1 的判定必须使用运行局 RNG 而非 Random.Shared，否则多人各客户端结果不一致造成情报不同步。
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        int alertAmount = (int)Amount;
        if (alertAmount >= 5)
        {
            int intelligence = alertAmount >= 15 ? 3 : alertAmount >= 10 ? 2 : 1;
            bool elite = room.RoomType == RoomType.Elite || room.RoomType == RoomType.Boss;
            if (Owner.Player?.RunState?.Rng?.Niche is { } niche)
            {
                if (niche.NextInt(100) < (elite ? 50 : 25))
                {
                    intelligence++;
                }
            }
            else if (Random.Shared.NextDouble() < (elite ? 0.5 : 0.25))
            {
                intelligence++;
            }
            BoostedRewardManager.QueueAlertConversionReward(Owner.Player!, intelligence);
        }
        await PowerCmd.Remove(this);
    }
}
