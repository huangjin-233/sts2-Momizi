#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
// ReSharper disable All

namespace Momizi.character.power;

[RegisterPower]
public class Helpunity: ModPowerTemplate
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Single;

    public bool Flag = true;
    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/Ready.png",
        BigIconPath: "res://images/powers/Ready.png"
    );
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel powers, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (powers == this)
        {
            if (Owner.Player != null)
            {
                CardModel? cardModel = await CardSelectCmd.FromHandForUpgrade(choiceContext, Owner.Player, this);
                if (cardModel != null)
                {
                    CardCmd.Upgrade(cardModel);
                }
            }

            await PowerCmd.Remove(this);
        }
    }
}