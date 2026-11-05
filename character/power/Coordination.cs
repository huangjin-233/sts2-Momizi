using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

/// <summary>
/// 同调：挂在每只白狼身上，无上限，战斗结束清零。
/// 回合中每只白狼分别结算：
///   - 本回合白狼攻击        → +1
///   - 本回合你为它提供过格挡 → +1
///   - 本回合替它承受过伤害   → +1
///   - 未达成                → -1
/// 三档读取：5+ / 10+ / 15+。
/// </summary>
[RegisterPower]
public class Coordination : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 可叠加：以层数表示同调值（无上限）
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/Coordination.png",
        BigIconPath: "res://images/powers/Coordination.png"
    );

    // 本回合该狼是否达成过同调 +1（攻击/格挡/代替承受），按狼独立判断，避免全局标志串扰
    public static bool firstattack;
    public static bool firstblock = true;
    
}