#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 钝化：本回合内，每当敌人的一次攻击被椛的格挡完全抵消，该敌人失去 Amount 层力量。仅由「钝化」卡施加，回合结束移除。
/// </summary>
[RegisterPower]
public class BluntPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/BluntPower.png",
        BigIconPath: "res://images/powers/BluntPower.png"
    );

    // 椛受到攻击伤害，被格挡抵消（未格挡伤害为 0）且来源是敌人时，削弱该敌人力量。
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || result.UnblockedDamage != 0)
        {
            return;
        }
        if (dealer?.IsEnemy != true || dealer.IsDead)
        {
            return;
        }
        // 敌人攻击被格挡完全抵消：使其失去 Amount 层力量
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), dealer, -Amount, Owner, null);
    }

    // 回合结束移除，保证仅对当次打出生效
    public override async Task AfterEnergyReset(Player player)
    {
        if (player == Owner.Player)
        {
            await PowerCmd.Remove(this);
        }
    }
}