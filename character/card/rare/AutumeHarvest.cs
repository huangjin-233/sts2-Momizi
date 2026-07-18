using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.rare;

[RegisterCard(typeof(MomiziCardPool))]
public class AutumeHarvest: ModCardTemplate
{
    
    public bool Remove;
    private const string _increaseKey = "Increase";
    // 基础耗能
    private const int energyCost = 3;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/rare/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(4, ValueProp.Move),
        ModCardVars.Int("CurrentUse",3),
        new IntVar("Increase", 1m)
    ];
    [SavedProperty]
    public int CurrentUse
    {
        get
        {
            return field;
        }
        set
        {
            AssertMutable();
            field = value;
            DynamicVars["CurrentUse"].BaseValue = field;
        }
    } = 3;

    [SavedProperty]
    public int IncreasedUse
    {
        get
        {
            return field;
        }
        set
        {
            AssertMutable();
            field = value;
        }
    }

    public AutumeHarvest() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    { 
        AbstractRoom currentRoom = CombatState.RunState.CurrentRoom;
        if (currentRoom is CombatRoom combatRoom)
        {
            int num1 = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 100);
            int num2 = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 100);
            int num3 = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 100);
            int num4 = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 100);
            if (num1 < 10)
            {
                combatRoom.AddExtraReward(Owner, new RelicReward(Owner));
                await PowerCmd.Apply<RelicPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
            }
            if (num2 < 25)await PotionCmd.TryToProcure(PotionFactory.CreateRandomPotionInCombat(Owner, Owner.RunState.Rng.CombatPotionGeneration).ToMutable(), Owner);
            if (num3 < 30)
            {
                combatRoom.AddExtraReward(Owner, new CardReward(CardCreationOptions.ForRoom(Owner, combatRoom.RoomType), 3, Owner));
                await PowerCmd.Apply<CardPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            }
            if (num4 < 50)await PlayerCmd.GainGold(20, Owner.Creature.Player);
            BuffFromPlay(1);
            (DeckVersion as AutumeHarvest)?.BuffFromPlay(1);
        }
        if (CurrentUse == 0)
        {
            await CardPileCmd.RemoveFromCombat(this, skipVisuals: false);
        }
    }
    public override async Task AfterCombatEnd(CombatRoom _)
    {
        if (CurrentUse == 0)
        {
            await CardPileCmd.RemoveFromDeck(this);
        }
    }
    protected override void OnUpgrade()
    {
        BuffFromPlay(-1);
        (DeckVersion as AutumeHarvest)?.BuffFromPlay(-1);
    }
    protected override void AfterDowngraded()
    {
        UpdateUse();
    }

    private void BuffFromPlay(int extraBlock)
    {
        IncreasedUse += extraBlock;
        UpdateUse();
    }

    private void UpdateUse()
    {
        CurrentUse = 3 - IncreasedUse;
    }
}