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

namespace Momizi.character.card.common;

// 突刺：0费/攻击/普通 消耗2层警戒：造成8点伤害；不足则5点。升级：造成11点伤害，不足则6点
[RegisterCard(typeof(MomiziCardPool))]
public class Thrust : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/common/{GetType().Name}.png"
    );

    protected override bool IsPlayable => Coordination.firstattack;
    protected override bool ShouldGlowGoldInternal => Coordination.firstattack;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(14, ValueProp.Move),
    ];

    public Thrust() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var damage =DynamicVars.Damage.BaseValue;
            await DamageCmd.Attack(damage)
                .FromCard(this,cardPlay)
                .Targeting(cardPlay.Target!)
                .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf)
    ];
}