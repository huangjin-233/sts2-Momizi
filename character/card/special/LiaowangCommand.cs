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
public class LiaowangCommand : ModCardTemplate, Command.IChoosable
{
    private const int energyCost = -1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Scry", 1)
    ];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/special/{GetType().Name}.png"
    );

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public LiaowangCommand() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Scry"].UpgradeValueBy(1);
    }

    public async Task OnChosen()
    {
        // 指令状态由白狼持有；预见效果仍由能力内部施加给其主人。
        if (Command.wolf != null)
        {
            await PowerCmd.Remove<JiebeiPower>(Command.wolf);
            await PowerCmd.Remove<YanhuPower>(Command.wolf);
            await PowerCmd.Apply<LiaowangPower>(new ThrowingPlayerChoiceContext(), Command.wolf,
                DynamicVars["Scry"].BaseValue, Owner.Creature, this);
        }
    }
}
