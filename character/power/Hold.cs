#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class Hold: ModPowerTemplate
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/Help.png",
        BigIconPath: "res://images/powers/Help.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(3, ValueProp.Move)
    ];
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel powers, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (powers == this)
        {
            await PowerCmd.Apply<BlurPower>(choiceContext, Owner, 1, Owner, null);
        }
    }
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (CombatManager.Instance.IsInProgress && target == Owner && result.UnblockedDamage > 0)
        {
            await CreatureCmd.GainBlock(Owner, DynamicVars.Block, null);//防御
        }
    }
    public override async Task AfterEnergyReset(Player player)
    {
        await PowerCmd.Remove(this);
    }
}