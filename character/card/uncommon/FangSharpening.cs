#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MinionLib.Targeting;
using Momizi.character.card.special;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 獠牙磨砺：1费/技能/罕见/消耗
// 选择一只白狼：选择获得2点力量或敏捷。该白狼同调6+：改为3点。升级：3点；同调6+ 4点。
[RegisterCard(typeof(MomiziCardPool))]
public class FangSharpening : ModCardTemplate
{
    // 被选中的白狼，供选项卡执行时使用
    public static Creature? wolf;

    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("StrengthAndDexterity", 2),
        ModCardVars.Int("StrengthAndDexterityHigh", 3)
    ];

    public FangSharpening() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        wolf = cardPlay.Target;
        if (wolf == null)
        {
            // 无有效目标：退还费用并回到手牌
            await PlayerCmd.GainEnergy(energyCost, Owner);
            await CardPileCmd.Add(cardPlay.Card, PileType.Hand, CardPilePosition.Random, cardPlay.Card, false);
            return;
        }

        // 创建力量/敏捷选项卡，玩家二选一
        CardModel strength = CombatState!.CreateCard<StrengthFangOption>(Owner);
        CardModel dexterity = CombatState.CreateCard<DexterityFangOption>(Owner);
        if (IsUpgraded)
        {
            strength.UpgradeInternal();
            dexterity.UpgradeInternal();
        }

        CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, new List<CardModel> { strength, dexterity }, Owner, false);
        if (selected is Command.IChoosable choosable)
        {
            await choosable.OnChosen();
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthAndDexterity"].UpgradeValueBy(1);
        DynamicVars["StrengthAndDexterityHigh"].UpgradeValueBy(1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.Coordinations),
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];
}
