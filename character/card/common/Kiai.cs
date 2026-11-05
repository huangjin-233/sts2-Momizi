using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Commands;
using MinionLib.Targeting;
using Momizi.character.power;
using Momizi.character.single;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.common;

// 气合：1费/技能/普通 —— 占位符，待实现
[RegisterCard(typeof(MomiziCardPool))]
public class Kiai : ModCardTemplate
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
    ];

    public Kiai() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }
    
    // 场上无同调≥8 的存活白狼时不可打（自动回手，不消耗能量）
    protected override bool IsPlayable =>
        Owner.Creature.Pets.Any(p =>
            p.IsAlive && p.GetPowerAmount<WolfVanguard>() > 0 && p.GetPowerAmount<Coordination>() >= 8);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        if (IsUpgraded)
        {
            await PowerCmd.Apply<KiaiPower>(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<KiaiPower>(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
        }
        if (cardPlay.Target.GetPowerAmount<Coordination>() >= 3)
            await PowerCmd.Apply<VigorPower>(choiceContext, cardPlay.Target, 3, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.Coordinations)
    ];
}