using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 示警：1费/技能/罕见 消耗3层：全体敌人1虚弱+1易伤；升级：2虚弱+2易伤
[RegisterCard(typeof(MomiziCardPool))]
public class WarningBurst : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(2),
        new PowerVar<VulnerablePower>(2)
    ];

    public WarningBurst() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.CombatState != null)
        {
            var enemies = Owner.Creature.CombatState.Enemies.Where(e => e.IsAlive);
            foreach (var enemy in enemies)
            {
                await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars.Weak.BaseValue, Owner.Creature, this);
                await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<Alert>()
    ];
}