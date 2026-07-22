using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.card.rare;
using Momizi.character.card.special;
using Momizi.character.card.uncommon;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
// ReSharper disable All

namespace Momizi.character.cards.basic
{
    [RegisterCard(typeof(MomiziCardPool))]
    
    public class SpellDuel : ModCardTemplate
    {
        // 基础耗能
        private const int energyCost = 1;
        // 卡牌类型
        private const CardType type = CardType.Attack;
        // 卡牌稀有度
        private const CardRarity rarity = CardRarity.Ancient;
        // 目标类型（AnyEnemy表示任意敌人）
        private const TargetType targetType = TargetType.AnyEnemy;
        // 是否在卡牌图鉴中显示
        private const bool shouldShowInCardLibrary = true;

        // 卡图资源
        public override CardAssetProfile AssetProfile => new(
            PortraitPath: $"res://images/cards/basic/{GetType().Name}.png"
        );
        // 卡牌基础数值
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new DamageVar(20, ValueProp.Move)
        ];

        public bool chew = false;
        public bool canaan = false;
        public bool bite = false;
        
        public SpellDuel() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        // 打出时的效果逻辑
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target!)
                .Execute(choiceContext);
            foreach (CardModel item in PileType.Hand.GetPile(base.Owner).Cards.Where((CardModel c) => c.IsUpgradable))
            {
                if (item is ChewOverAndSavor)
                {
                    chew = true;
                }
                if (item is RabiesBite)
                {
                    bite = true;
                }
                if (item is ExpelleesCanaan)
                {
                    canaan = true;
                }
            }
            if (!chew)
            {
                IEnumerable<CardModel> chew = await ChewOverAndSavor.CreateInHand(Owner, 1, CombatState);
            }
            if (!bite)
            {
                IEnumerable<CardModel> bite = await RabiesBite.CreateInHand(Owner, 1, CombatState);
            }
            if (!canaan)
            {
                IEnumerable<CardModel> canaan = await ExpelleesCanaan.CreateInHand(Owner, 1, CombatState);
            }
            IReadOnlyList<CardModel> cards1 = PileType.Draw.GetPile(Owner).Cards.Where((CardModel c) => c.Keywords.Contains(MomiziKeyword.Spell)).ToList();
            IReadOnlyList<CardModel> cards2 = PileType.Discard.GetPile(Owner).Cards.Where((CardModel c) => c.Keywords.Contains(MomiziKeyword.Spell)).ToList();
            IReadOnlyList<CardModel> cards3 = PileType.Hand.GetPile(Owner).Cards.Where((CardModel c) => c.Keywords.Contains(MomiziKeyword.Spell)).ToList();
            List<CardModel> cards = new List<CardModel>();
            cards.AddRange(cards1);
            cards.AddRange(cards2);
            cards.AddRange(cards3);
            foreach (CardModel card in cards)
            {
                CardCmd.Upgrade(card);
            }
        }

        // 升级后的效果逻辑
        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(5);
        }
        protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
            HoverTipFactory.FromCard<ChewOverAndSavor>(),
            HoverTipFactory.FromCard<RabiesBite>(),
            HoverTipFactory.FromCard<ExpelleesCanaan>(),
            HoverTipFactory.FromKeyword(MomiziKeyword.Spell)
        ];
    }
}
