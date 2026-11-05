using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Commands;
using MinionLib.Minion;
using Momizi.character.Mini;
using Momizi.character.card.special;
using Momizi.character.power;
using Momizi.character.relics;
using Momizi.character.single;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.basic;

[RegisterCard(typeof(MomiziCardPool))]
[RegisterCharacterStarterCard(typeof(Momizi))]
public class WhiteWolfGuard : ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 0;
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
    public WhiteWolfGuard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];
    public static CardModel coomd1;

    public static CardModel coomd2;
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 一次召唤一条白狼，上限 2 条，优先召唤 WolfOne（缺一号召一号，缺二号召二号）。
        // 白狼最大生命以实际拥有的天狗遗物持久化的 baseMaxHp 为准（特训等卡可提升）。
        decimal wolfMaxHp1 = Owner.GetRelic<TenguSee>() is { } t1
            ? t1.DynamicVars.MaxHp.BaseValue
            : Owner.GetRelic<MountainSee>() is { } m1
                ? m1.DynamicVars.MaxHp.BaseValue
                : DynamicVars.MaxHp.BaseValue;
        decimal wolfMaxHp2 = Owner.GetRelic<TenguSee>() is { } t2
            ? t2.DynamicVars["Max"].BaseValue
            : Owner.GetRelic<MountainSee>() is { } m2
                ? m2.DynamicVars["Max"].BaseValue
                : DynamicVars["Max"].BaseValue;
        // 白狼基础力量以遗物持久化的 baseStrength 为准（天狗三档奖励叠加，跨战斗保留）
        decimal wolfStrength1 = Owner.GetRelic<TenguSee>() is { } s1
            ? s1.DynamicVars["Power1"].BaseValue
            : Owner.GetRelic<MountainSee>() is { } ms1
                ? ms1.DynamicVars["Power1"].BaseValue
                : 0m;
        decimal wolfStrength2 = Owner.GetRelic<TenguSee>() is { } s2
            ? s2.DynamicVars["Power2"].BaseValue
            : Owner.GetRelic<MountainSee>() is { } ms2
                ? ms2.DynamicVars["Power2"].BaseValue
                : 0m;
        Creature wolf1 = null;
        Creature wolf2 = null;
        if (!WolfOne.OwnedBy(Owner))
        {
            wolf1 = await MinionCmd.AddMinion<WolfOne>(choiceContext, Owner, new MinionSummonOptions(
                MaxHp: wolfMaxHp1,
                PrimaryStatAmount: wolfStrength1 + (IsUpgraded ? 2 : 0),
                Source: this,
                Position: MinionPosition.Front));
        }
        else if (!WolfTwo.OwnedBy(Owner))
        {
            wolf2 = await MinionCmd.AddMinion<WolfTwo>(choiceContext, Owner, new MinionSummonOptions(
                MaxHp: wolfMaxHp2,
                PrimaryStatAmount: wolfStrength2 + (IsUpgraded ? 2 : 0),
                Source: this,
                Position: MinionPosition.FrontUpper));
        }
        await PowerCmd.Apply<WolfGuard>(choiceContext,Owner.Creature,DynamicVars["Attack"].BaseValue,Owner.Creature,this);

        // 生成「指令」卡牌；白狼卫队升级时，指令也为升级版。
        if (CombatState != null)
        {
            var command = CombatState.CreateCard<Command>(Owner);
            if (IsUpgraded)
            {
                command.UpgradeInternal();
            }
            if (coomd1 == null)
            {
                coomd1 = command;
            }
            else
            {
                coomd2 = command;
            }
            await CardPileCmd.AddGeneratedCardToCombat(command, PileType.Hand, Owner);
        }
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }
    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf),
        HoverTipFactory.FromCard<Command>(IsUpgraded)
    ];
}