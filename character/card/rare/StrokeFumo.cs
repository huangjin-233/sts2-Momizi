using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MinionLib.Targeting;
using Momizi.character.Mini;
using Momizi.character.power;
using Momizi.character.relics;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.rare;

// 抚摸fumo：2费/技能/稀有
// 选择一只白狼：其永久增加（同调÷2+3）点最大生命。此牌打出5次后从牌组中永久移除。升级：1费。

[RegisterCard(typeof(MomiziCardPool))]
public class StrokeFumo : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/rare/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Computed("HpGain",3 , (card, target) => card.DynamicVars["HpGain"].BaseValue + (target?.GetPowerAmount<Coordination>()/2 ?? 0))
    ];
    public StrokeFumo() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
        {
            // 无有效目标：退还费用并回到手牌
            await PlayerCmd.GainEnergy(energyCost, Owner);
            await CardPileCmd.Add(cardPlay.Card, PileType.Hand, CardPilePosition.Random, cardPlay.Card, false);
            return;
        }

        // 增益 = 同调层数÷2 + 3
        decimal gain = DynamicVars.EvaluateValueOrDefault("HpGain", target: cardPlay.Target);

        // 狼1：参考原版召唤白狼，以遗物持久化的 BaseValue（永久基准）提升最大生命
       if (target.GetPowerAmount<WolfSlot>() == 1)
        {
            if (Owner.GetRelic<TenguSee>() is { } tengu)
            {
                await tengu.PersistMaxHp1((int)(tengu.DynamicVars.MaxHp.BaseValue + gain));
                tengu.Flash();
            }
            if (Owner.GetRelic<MountainSee>() is { } mountain)
            {
                await mountain.PersistMaxHp1((int)(mountain.DynamicVars.MaxHp.BaseValue + gain));
                mountain.Flash();
            }
        }
        // 狼2：提升遗物中 Max 对应的生命上限
        else if (target.GetPowerAmount<WolfSlot>() == 2)
        {
            if (Owner.GetRelic<TenguSee>() is { } tengu)
            {
                await tengu.PersistMaxHp2((int)(tengu.DynamicVars["Max"].BaseValue + gain));
                tengu.Flash();
            }
            if (Owner.GetRelic<MountainSee>() is { } mountain)
            {
                await mountain.PersistMaxHp2((int)(mountain.DynamicVars["Max"].BaseValue + gain));
                mountain.Flash();
            }
        }
        await CardPileCmd.RemoveFromCombat(this);
    }
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
    
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf),
        HoverTipFactory.FromPower<Coordination>()
    ];
}
