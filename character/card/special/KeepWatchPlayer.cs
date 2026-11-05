using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Momizi.character.card.uncommon;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.special
{
    // 守望：1费/技能/罕见 —— 占位符，待实现  多人专属
    [RegisterCard(typeof(TokenCardPool))]
    public class KeepWatchPlayer : ModCardTemplate
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Token;
        private const TargetType targetType = TargetType.AnyAlly;
        private const bool shouldShowInCardLibrary = true;

        public override CardAssetProfile AssetProfile => new(
            PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
        );

        public static Creature player;
        private Creature wolf;
        protected override IEnumerable<DynamicVar> CanonicalVars => [];

        public KeepWatchPlayer() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (wolf != null && player != null)
            {
                await PowerCmd.Remove<KeepWatchPowerPlayer>(player);
            }
            player = cardPlay.Target;
            wolf = KeepWatch.wolf;
            await PowerCmd.Apply<KeepWatchPower>(choiceContext, wolf, 1, Owner.Creature, this);
            if (player != null)
                await PowerCmd.Apply<KeepWatchPowerPlayer>(choiceContext, player, 1, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
        }

        protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];
    }
}