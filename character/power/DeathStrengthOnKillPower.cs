#nullable enable
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 强化遭遇词条「每当一个敌人死亡，其余敌人获得 3 点力量」：挂在每个敌人身上，
/// 任一敌人死亡时，场地里其他存活敌人获得 3 点力量。
/// </summary>
[RegisterPower]
public class DeathStrengthOnKillPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => false;

    /// <summary>死亡时给其余敌人增加的力量。</summary>
    public const int Grant = 3;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/Alert.png",
        BigIconPath: "res://images/powers/Alert.png"
    );

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature dead, bool wasDead,
        float damageSource)
    {
        if (dead == Owner || !dead.IsAlive)
        {
            return;
        }
        // 只有敌人死亡时触发；对场地其他存活敌人加力量
        var combat = Owner.CombatState;
        if (combat == null)
        {
            return;
        }
        foreach (var enemy in combat.HittableEnemies)
        {
            if (enemy != dead && enemy.IsAlive)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, Grant, dead, null);
            }
        }
    }
}