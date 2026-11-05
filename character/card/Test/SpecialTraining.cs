using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Targeting;
using Momizi.character.Mini;
using Momizi.character.power;
using Momizi.character.relics;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.Test;

// 特训：0费/技能/普通 本场战斗，[gold]大天狗[/gold]的生命上限+{MaxHpGain:diff()}。
[RegisterCard(typeof(MomiziCardPool))]
public class SpecialTraining : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/common/{GetType().Name}.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("MaxHpGain", 3)
    ];

    public SpecialTraining() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int gain = DynamicVars["MaxHpGain"].IntValue;
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

    protected override void OnUpgrade()
    {
        DynamicVars["MaxHpGain"].UpgradeValueBy(2);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf)
    ];
}