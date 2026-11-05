#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.special;

// 牵制：指令的衍生命令之一。白狼获得50%减伤，伤害改为由其承担；升级后额外获得格挡。
[RegisterCard(typeof(TokenCardPool))]
public class QianzhiCommand : ModCardTemplate, Command.IChoosable
{
    private const int energyCost = -1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Block", 0)
    ];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/special/{GetType().Name}.png"
    );

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public QianzhiCommand() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
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
            // 与其他命令互斥：选中牵制，移除白狼身上其它命令效果。
            await PowerCmd.Remove<LiaowangPower>(Command.wolf);
            await PowerCmd.Remove<JiebeiPower>(Command.wolf);
            await PowerCmd.Remove<YanhuPower>(Command.wolf);
            await PowerCmd.Apply<PinPower>(new ThrowingPlayerChoiceContext(), Command.wolf, 1,
                Owner.Creature, this);
            if (IsUpgraded)
            {
                await CreatureCmd.GainBlock(Command.wolf, DynamicVars["Block"].BaseValue, ValueProp.Unpowered, null);
            }
        }
    }
}