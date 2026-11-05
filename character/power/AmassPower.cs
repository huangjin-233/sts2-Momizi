#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

// 蓄锐：每连续打出3张非攻击牌，椛获得4点活力与 Amount(1/升级2) 点力量；打出攻击牌后计数归零。
// Amount 由卡牌传入 = 力量值（升级 1→2）。
[RegisterPower]
public class AmassPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/AmassPower.png",
        BigIconPath: "res://images/powers/AmassPower.png"
    );

    // 当前连续打出的非攻击牌数
    private int _counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只统计本玩家打出的牌
        if (cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        // 打出攻击牌：重新计数
        if (cardPlay.Card.Type == CardType.Attack)
        {
            _counter = 0;
            return;
        }

        // 非攻击牌：计数 +1，累计 3 张时发放效果并重置
        _counter++;
        if (_counter >= 3)
        {
            await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Owner, 4, Owner, null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
            _counter = 0;
        }
    }
}