using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
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
        IconPath: "res://Test/images/powers/Help.png",
        BigIconPath: "res://Test/images/powers/Help.png"
    );
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Power && cardPlay.Card.Owner != Owner.Player)
        {
            await PowerCmd.Apply<FreePowerPower>(choiceContext,Owner, 1, Owner, null);
            await PowerCmd.Decrement(this);
        }
        if (cardPlay.Card.Type == CardType.Skill && cardPlay.Card.Owner != Owner.Player)
        {
            await PowerCmd.Apply<HelpSkill>(choiceContext,Owner, 2, Owner, null);
            await PowerCmd.Decrement(this);
        }
        if (cardPlay.Card.Type == CardType.Attack && cardPlay.Card.Owner != Owner.Player)
        {
            await PowerCmd.Apply<HelpAttack>(choiceContext,Owner, 2, Owner, null);
            await PowerCmd.Decrement(this);
        }
    }
    public override async Task AfterEnergyReset(Player player)
    {
        await PowerCmd.Remove(this);
    }
}