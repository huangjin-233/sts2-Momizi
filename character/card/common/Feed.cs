using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Targeting;
using Momizi.character.Mini;
using Momizi.character.power;
using Momizi.character.relics;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.common;

// 喂食：1费/技能/普通 —— 占位符，待实现
[RegisterCard(typeof(MomiziCardPool))]
public class Feed : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/common/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>(2),
        ModCardVars.Int("Plating",4),
        new EnergyVar(1),
        new CardsVar(1),
        ModCardVars.Int("MaxHp",5)
    ];

    public Feed() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (CardModel item in await CardSelectCmd.FromHand(choiceContext, Owner,
                     new CardSelectorPrefs(SelectionScreenPrompt, 1), null, this))
            //选择手牌
        {
            await CardCmd.Exhaust(choiceContext, item);
            if (cardPlay.Target != null)
            {
                if (item.Type == CardType.Attack)
                    await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, DynamicVars.Strength.BaseValue,
                        Owner.Creature, this);
                else if (item.Type == CardType.Skill)
                {
                    await PowerCmd.Apply<WolfPlatingPower>(choiceContext, cardPlay.Target,
                        DynamicVars["Plating"].BaseValue,
                        Owner.Creature, this);
                }
                else if (item.Type == CardType.Power)
                {
                    int gain = DynamicVars["MaxHp"].IntValue;
                    var target = cardPlay.Target;

                    // 狼1：提升遗物中 MaxHp 对应的生命上限
                   if (target.GetPowerAmount<WolfSlot>() == 1)
                    {
                        if (Owner.GetRelic<TenguSee>() is { } tengu)
                        {
                            await tengu.PersistMaxHp1(tengu.DynamicVars.MaxHp.IntValue + gain);
                            tengu.Flash();
                        }
                        if (Owner.GetRelic<MountainSee>() is { } mountain)
                        {
                            await mountain.PersistMaxHp1(mountain.DynamicVars.MaxHp.IntValue + gain);
                            mountain.Flash();
                        }
                    }
                    // 狼2：提升遗物中 Max 对应的生命上限
                    else if (target.GetPowerAmount<WolfSlot>() == 2)
                    {
                        if (Owner.GetRelic<TenguSee>() is { } tengu)
                        {
                            await tengu.PersistMaxHp2(tengu.DynamicVars["Max"].IntValue + gain);
                            tengu.Flash();
                        }
                        if (Owner.GetRelic<MountainSee>() is { } mountain)
                        {
                            await mountain.PersistMaxHp2(mountain.DynamicVars["Max"].IntValue + gain);
                            mountain.Flash();
                        }
                    }
                }
                else
                {
                    await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
                    if (IsUpgraded) await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1);
        DynamicVars["Plating"].UpgradeValueBy(2);
        DynamicVars["MaxHp"].UpgradeValueBy(3);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    HoverTipFactory.FromPower<StrengthPower>(),
    HoverTipFactory.FromPower<WolfPlatingPower>(),
    HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf)
    ];
}