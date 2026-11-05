#nullable enable
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class BattleReportPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/BattleReportPower.png",
        BigIconPath: "res://images/powers/BattleReportPower.png"
    );

    private decimal consumedAlert;
    private bool isProcessing;
    private decimal pendingAlertGain;

    public async Task TriggerThresholdRewards(PlayerChoiceContext choiceContext, decimal before, decimal delta)
    {
        if (delta >= 0)
        {
            return;
        }

        int beforeThresholds = (int)before / 6;
        int afterThresholds = Math.Max(0, (int)(before + delta)) / 6;
        int triggerCount = Math.Max(0, beforeThresholds - afterThresholds);
        for (int i = 0; i < triggerCount; i++)
        {
            if (Owner.Player != null)
            {
                await PlayerCmd.GainEnergy(1, Owner.Player);
                await CardPileCmd.Draw(choiceContext, 1, Owner.Player);
            }
            pendingAlertGain += 6;
        }
    }

    public async Task ApplyPendingAlertGain(PlayerChoiceContext choiceContext)
    {
        if (pendingAlertGain <= 0)
        {
            return;
        }

        var amount = pendingAlertGain;
        pendingAlertGain = 0;
        await PowerCmd.Apply<Alert>(choiceContext, Owner, amount, Owner, null);
    }
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (power is not Alert || amount >= 0)
        {
            return;
        }

        var alert = (Alert)power;
        if (alert.IsClampingToMax)
        {
            return;
        }

        if (isProcessing)
        {
            consumedAlert -= amount;
            return;
        }

        isProcessing = true;
        try
        {
            consumedAlert -= amount;
            while (consumedAlert >= 6)
            {
                consumedAlert -= 6;
                if (Owner.Player != null)
                {
                    await PlayerCmd.GainEnergy(1, Owner.Player);
                    await CardPileCmd.Draw(choiceContext, 1, Owner.Player);
                }
                await PowerCmd.Apply<Alert>(choiceContext, Owner, 6, Owner, null);
            }
        }
        finally
        {
            isProcessing = false;
        }
    }
}
