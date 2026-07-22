using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.basic;
using Momizi.character.card.uncommon;
using Momizi.character.cards.basic;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

public class WolfGuard: ModPowerTemplate
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Single;
    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/WolfGuard.png",
        BigIconPath: "res://images/powers/WolfGuard.png"
    );

    public bool first = true;

    // 每回合首次打出攻击牌时，白狼对同一目标造成伤害。
    // 白狼为伤害来源：受你的力量与白狼自身力量加成，不受你的负面效果（如虚弱）影响。
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack || !first)
        {
            return;
        }
        var wolf = WhiteWolfGuard.whitewolf;
        if (wolf == null || !wolf.IsAlive || cardPlay.Target == null)
        {
            return;
        }

        first = false;
        decimal amount = Amount + Owner.GetPowerAmount<StrengthPower>();
        await Task.Delay(350);
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, amount, ValueProp.Move, wolf);
    }

    public override async Task AfterEnergyReset(Player player)
    {
        first = true;
    }
}
