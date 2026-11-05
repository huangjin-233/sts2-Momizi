using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 赌注：1费/技能/罕见
// 失去当前全部格挡；下回合开始时获得其 1.5 倍（向下取整）的格挡。升级：保留本效果不失去格挡。
[RegisterCard(typeof(MomiziCardPool))]
public class Wager : ModCardTemplate
{
    public override bool GainsBlock => true;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Multiplier", 2m) // 2 倍
    ];

    public Wager() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner.Creature;
        int currentBlock = owner.Block;

        // 失去当前全部格挡
        await CreatureCmd.LoseBlock(choiceContext,owner, currentBlock,Owner.Creature);

        // 下回合开始获得其 2 倍（向下取整）格挡
        decimal pending = currentBlock * DynamicVars["Multiplier"].BaseValue;
        if (pending > 0)
        {
            await PowerCmd.Apply<WagerPower>(choiceContext, owner, pending, owner, this);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];
}