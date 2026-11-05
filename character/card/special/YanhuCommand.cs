#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Momizi.character.card.basic;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.special;

[RegisterCard(typeof(TokenCardPool))]
public class YanhuCommand : ModCardTemplate, Command.IChoosable
{
    private const int energyCost = -1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Block", -1)
    ];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/special/{GetType().Name}.png"
    );

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public YanhuCommand() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(5);
    }

    public async Task OnChosen()
    {
        if (Command.wolf != null)
        {
            await PowerCmd.Remove<LiaowangPower>(Command.wolf);
            await PowerCmd.Remove<JiebeiPower>(Command.wolf);
            await PowerCmd.Apply<YanhuPower>(new ThrowingPlayerChoiceContext(), Command.wolf,
                DynamicVars["Block"].BaseValue, Owner.Creature, this);
            await PowerCmd.Remove<PinPower>(Command.wolf);
        }
    }
}
