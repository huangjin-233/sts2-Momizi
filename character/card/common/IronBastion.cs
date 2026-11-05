#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.Mini;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.common;

// 铁壁：1费/技能/普通 —— 模板，待实现
// 获得8格挡；若上个回合敌人未对你的白狼造成伤害，所有白狼同调+1。升级：11格挡。
[RegisterCard(typeof(MomiziCardPool))]
public class IronBastion : ModCardTemplate
{
    public override bool GainsBlock => true;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/common/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8, ValueProp.Move),
        ModCardVars.Int("SyncGain", 1)
    ];

    public IronBastion() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        // 上个回合敌人未对白狼造成实际伤害 → 所有白狼同调 +1
        if (!WolfVanguard.AnyWolfDamagedLastTurn)
        {
            var wolves = new Creature?[] { WolfOne.Current?.Creature, WolfTwo.Current?.Creature };
            foreach (var wolf in wolves)
            {
                if (wolf != null && wolf.IsAlive && wolf.GetPowerAmount<WolfVanguard>() > 0)
                {
                    await PowerCmd.Apply<Coordination>(choiceContext, wolf, DynamicVars["SyncGain"].IntValue, Owner.Creature, this);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
         HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf)
    ];
}