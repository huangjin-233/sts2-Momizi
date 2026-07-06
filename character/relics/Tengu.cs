#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Teamy.character;

namespace Momizi.character.relics;
[RegisterRelic(typeof(MomiziRelicPool))]
[RegisterCharacterStarterRelic(typeof(Momizi))]
public class Tengu: ModRelicTemplate
{
    // 稀有度
    public override RelicRarity Rarity => RelicRarity.Starter;
    

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://Test/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://Test/images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://Test/images/relics/{GetType().Name}.png"
    );
    // 每回合开始时，抽一张牌
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Type == PowerType.Debuff)
        {
            if (!Dodge.Active)
            {
                Flash();
                await PowerCmd.Apply<Alert>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
            }
        }

    }
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (CombatManager.Instance.IsInProgress && target == Owner.Creature && result.UnblockedDamage > 0 &&
            !Owner.Creature.HasPower<Dodge>())
        {
            if (!Dodge.Active)
            {
                await PowerCmd.Apply<Alert>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
            }
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        await PowerCmd.Apply<Help>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
        if (side == Owner.Creature.Side && combatState.RoundNumber == 1)
        {
            Flash(); // 触发遗物图标闪烁
            IEnumerable<Creature> enumerable = Owner.Creature.CombatState.PlayerCreatures.Where((Creature c) => c?.IsAlive ?? false).ToList();
            int count = enumerable.Count()-1;
            if (count == 0)
            {
                await PowerCmd.Apply<JiZhong>(new ThrowingPlayerChoiceContext(), Owner.Creature, 2, Owner.Creature, null);
            }
            else if (count == 1)
            {
                await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
            }
            else if (count == 2)
            {
                await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
            }
            else
            {
                await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
                await PowerCmd.Apply<JiZhong>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
            }
        }
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromKeyword(MomiziKeyword.Loyalty),
        HoverTipFactory.FromKeyword(MomiziKeyword.SelfEsteem),
        HoverTipFactory.FromKeyword(MomiziKeyword.Collaboration),
    ];
}