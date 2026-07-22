using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Commands;
using MinionLib.Minion;
using Momizi.character.Mini;
using Momizi.character.card.special;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.basic;

[RegisterCard(typeof(MomiziCardPool))]
[RegisterCharacterStarterCard(typeof(Momizi))]
public class WhiteWolfGuard : ModCardTemplate
{
    public override bool GainsBlock => true;
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Power;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Basic;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/basic/{GetType().Name}.png"
    );
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new MaxHpVar(10),
        ModCardVars.Int("Attack",4)
    ];
    public static Creature whitewolf;
    public WhiteWolfGuard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var Wolf = await MinionCmd.AddMinion<Wolf>(choiceContext, Owner, new MinionSummonOptions(
            MaxHp: DynamicVars.MaxHp.BaseValue,
            PrimaryStatAmount: 0,
            Source: this,
            Position: MinionPosition.Front));
        whitewolf = Wolf;
        await PowerCmd.Apply<WolfGuard>(choiceContext,Owner.Creature,DynamicVars["Attack"].BaseValue,Owner.Creature,this);
        Mini.Wolf.wolf = true;

        // 生成「指令」卡牌；白狼卫队升级时，指令也为升级版。
        var command = CombatState.CreateCard<Command>(Owner);
        if (IsUpgraded)
        {
            command.UpgradeInternal();
        }
        await CardPileCmd.AddGeneratedCardToCombat(command, PileType.Hand, Owner);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.MaxHp.UpgradeValueBy(4);
        DynamicVars["Attack"].UpgradeValueBy(1);
    }
}