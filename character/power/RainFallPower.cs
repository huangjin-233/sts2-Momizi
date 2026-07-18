using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.relics;
using STS2RitsuLib.Combat.HealthBars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using YakumoAkai.character.power;

namespace Momizi.character.power;

public class RainFallPower : ModPowerTemplate,IHealthBarForecastSource
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Debuff;

    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/RainFallPower.png",
        BigIconPath: "res://images/powers/RainFallPower.png"
    );

    public override bool AllowNegative => false;

    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        return HealthBarForecasts.Single(
            Amount*2, // 展示的数量（例如如果你的能力有2倍效果可以乘2）
            new Color(0f, 1f, 1f), // 颜色
            HealthBarForecastGrowthDirection.FromRight // 从左边开始延伸还是右边开始
        );
    }

    private int TriggerCount
    {
        get
        {
            IEnumerable<Creature> source = from c in Owner.CombatState.GetOpponentsOf(Owner)
                where c.IsAlive
                select c;
            return Math.Min(base.Amount, 1 + source.Sum((a) => a.GetPowerAmount<Rain>()));
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)

    {
        int iterations = TriggerCount;
        if (side != Owner.Side)
        {
            for (int i = 0; i < 3; i++)
            {
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, Amount/3,
                    ValueProp.Unblockable | ValueProp.Unpowered, null, null);
            }
        }

        if (side == Owner.Side)
        {
            {
                for (int i = 0; i < 3; i++)
                {
                    await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, Amount/3,
                        ValueProp.Unblockable | ValueProp.Unpowered, null, null);
                }
            }
        }
    }
}