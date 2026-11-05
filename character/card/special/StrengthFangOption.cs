using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using Momizi.character.card.uncommon;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.special;

// 獠牙磨砺选项卡：获得力量
[RegisterCard(typeof(TokenCardPool))]
public class StrengthFangOption : ModCardTemplate, Command.IChoosable
{
    private const int energyCost = -1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("StrengthAndDexterity", 2),
        ModCardVars.Int("StrengthAndDexterityHigh", 3)
    ];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/special/{GetType().Name}.png"
    );

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public StrengthFangOption() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthAndDexterity"].UpgradeValueBy(1);
        DynamicVars["StrengthAndDexterityHigh"].UpgradeValueBy(1);
    }

    public async Task OnChosen()
    {
        if (FangSharpening.wolf is { } target)
        {
            int StrengthAndDexterity = target.GetPowerAmount<Coordination>() >= 6
                ? DynamicVars["StrengthAndDexterityHigh"].IntValue
                : DynamicVars["StrengthAndDexterity"].IntValue;
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, StrengthAndDexterity, Owner.Creature, this);
        }
    }
}
