#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;
[RegisterPower]
public class QuickThinkDefendPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Single;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/Alert.png",
        BigIconPath: "res://images/powers/Alert.png"
    );
    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        List<CardModel> items = PileType.Hand.GetPile(Owner.Player).Cards.Where(c => c.Type == CardType.Skill && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList();
        foreach (CardModel cardModel in items)
        {
            cardModel.EnergyCost.UpgradeBy(1);
        }
    }
    
    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!props.IsCardOrMonsterMove())
        {
            return 1m;
        }
        if (cardSource == null)
        {
            return 1m;
        }
        if (cardSource.Owner != Owner.Player)
        {
            return 1m;
        }
        return 2m;
    }

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        await PowerCmd.Remove(this);
    }
}