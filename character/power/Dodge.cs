using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.power;

[RegisterPower]
public class Dodge: ModPowerTemplate
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/powers/Dodge.png",
        BigIconPath: "res://images/powers/Dodge.png"
    );

    public static bool Active;
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel powers, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (powers.Type == PowerType.Debuff && applier.IsEnemy)
        {
            await PowerCmd.Remove(powers);
            if (Owner.Player.Creature.HasPower<StrikeAfterPower>())
            {
                IEnumerable<CardModel> distinctForCombat= CardFactory.GetDistinctForCombat(
                    Owner.Player, 
                    from c in Owner.Player.Character.CardPool.GetUnlockedCards(
                        Owner.Player.UnlockState, 
                        Owner.Player.Creature.CombatState.RunState.CardMultiplayerConstraint)
                    where c.Rarity == CardRarity.Common
                    where c.Type == CardType.Attack
                    select c,
                    1,
                    Owner.Player.Creature.CombatState.RunState.Rng.CombatCardGeneration).ToList();
            }
            await PowerCmd.Decrement(this);
            Active = true;
        }
    }
    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
        {
            return amount;
        }
        return 0m;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        Active = true;
        await PowerCmd.Decrement(this);
    }
    public override async Task AfterEnergyReset(Player player)
    {
        await PowerCmd.Remove(this);
    }
}