#nullable enable
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
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

    // 警戒上限 20（场景覆盖层 Alerts.cs 也引用该字段）
    public static int max = 20;

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
    // 警戒不衰减、不触发爆炸；仅做上限钳制。
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && Amount > max)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, max - Amount, applier, cardSource);
        }
    }

    // 战斗结束：按档位转化为情报（≥15→3、≥10→2、≥5→1；25% 额外 +1，精英战 50%）。
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        int alertAmount = (int)Amount;
        if (alertAmount >= 5)
        {
            int intelligence = alertAmount >= 15 ? 3 : alertAmount >= 10 ? 2 : 1;
            bool elite = room.RoomType == RoomType.Elite || room.RoomType == RoomType.Boss;
            if (Random.Shared.NextDouble() < (elite ? 0.5 : 0.25))
            {
                intelligence++;
            }
            Owner.Player?.GetRelic<Tengu>()?.AddIntelligence(intelligence);
        }
        await PowerCmd.Remove(this);
    }
}
