#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Momizi.character.card.basic;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.special;

[RegisterCard(typeof(MomiziCardPool))]
public class JiebeiCommand : ModCardTemplate, Command.IChoosable
{
    private const int energyCost = -1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Alert", 2)
    ];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/special/{GetType().Name}.png"
    );

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public JiebeiCommand() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Alert"].UpgradeValueBy(1);
    }

    public async Task OnChosen()
    {
        var wolf = WhiteWolfGuard.whitewolf;
        if (wolf is not { IsAlive: true })
        {
            return;
        }

        await PowerCmd.Remove<LiaowangPower>(wolf);
        await PowerCmd.Remove<YanhuPower>(wolf);
        await PowerCmd.Apply<JiebeiPower>(new ThrowingPlayerChoiceContext(), wolf, DynamicVars["Alert"].BaseValue, Owner.Creature, this);
    }
}
