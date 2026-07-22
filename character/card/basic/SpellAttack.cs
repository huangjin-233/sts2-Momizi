using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character;
using Momizi.character.card.special;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
// ReSharper disable All

namespace Momizi.character.cards.basic
{
    [RegisterCard(typeof(MomiziCardPool))]
    [RegisterCharacterStarterCard(typeof(Momizi))]
    [RegisterArchaicToothTranscendence(typeof(SpellDuel))]
    public class SpellAttack : ModCardTemplate
    {
        // 基础耗能
        private const int energyCost = 1;
        // 卡牌类型
        private const CardType type = CardType.Attack;
        // 卡牌稀有度
        private const CardRarity rarity = CardRarity.Basic;
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
            new DamageVar(9, ValueProp.Move)
        ];

        public int num = 0;
        
        public SpellAttack() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        // 打出时的效果逻辑
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target!)
                .Execute(choiceContext);
            if (num == 0)
            {
                IEnumerable<CardModel> Rabiesbite = await RabiesBite.CreateInHand(base.Owner, 1, base.CombatState);
                num++;
            }
            else
            {
                IReadOnlyList<CardModel> cards1 = PileType.Draw.GetPile(Owner).Cards.Where((CardModel c) => c.Keywords.Contains(MomiziKeyword.Spell)).ToList();
                IReadOnlyList<CardModel> cards2 = PileType.Discard.GetPile(Owner).Cards.Where((CardModel c) => c.Keywords.Contains(MomiziKeyword.Spell)).ToList();
                IReadOnlyList<CardModel> cards3 = PileType.Hand.GetPile(Owner).Cards.Where((CardModel c) => c.Keywords.Contains(MomiziKeyword.Spell)).ToList();
                List<CardModel> cards = new List<CardModel>();
                cards.AddRange(cards1);
                cards.AddRange(cards2);
                cards.AddRange(cards3);
                IEnumerable<CardModel> enumerable = cards.ToList().UnstableShuffle(base.Owner.Creature.Player.RunState.Rng.CombatCardSelection).Take(1);
                foreach (CardModel card in enumerable)
                {
                    CardCmd.Upgrade(card);
                }
            }
                
        }

        // 升级后的效果逻辑
        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3);
        }
        protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
            HoverTipFactory.FromCard<RabiesBite>(),
            HoverTipFactory.FromKeyword(MomiziKeyword.Spell)
        ];
    }
}
