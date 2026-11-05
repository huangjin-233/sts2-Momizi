#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MinionLib.Targeting;
using Momizi.character.Mini;
using Momizi.character.power;
using Momizi.character.relics;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 交托：2费/技能/罕见
// 本回合无法获得格挡；选择一只白狼获得「被瞄准」，且永久 +3（升级 +5）最大生命。
[RegisterCard(typeof(MomiziCardPool))]
public class Entrust : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private static readonly TargetType targetType = MinionTargetTypes.AnyMinion;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("MaxHpGain", 6)
    ];

    public Entrust() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
        {
            // 无有效目标：退还费用并回到手牌
            await PlayerCmd.GainEnergy(energyCost, Owner);
            await CardPileCmd.Add(cardPlay.Card, PileType.Hand, CardPilePosition.Random, cardPlay.Card, false);
            return;
        }

        // 清除椛本体与各白狼身上原有的「被瞄准」预判标记，保证重定向目标唯一
        await ClearExistingAimedAt(choiceContext);

        // 本回合无法获得格挡
        await PowerCmd.Apply<NoBlockPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);

        // 目标白狼获得「被瞄准」（仅此卡生效）：敌方单体攻击指向椛/另一只白狼时，改打被瞄准的这只狼
        await PowerCmd.Apply<EntrustMarkedPower>(choiceContext, target, 1, Owner.Creature, this);

        // 永久提升该白狼最大生命（经遗物持久化，战斗内外均保留）
        int gain = DynamicVars["MaxHpGain"].IntValue;
        if (target.GetPowerAmount<WolfSlot>() == 1)
        {
            if (Owner.GetRelic<TenguSee>() is { } tengu)
            {
                await tengu.PersistMaxHp1(tengu.DynamicVars.MaxHp.IntValue + gain);
                tengu.Flash();
            }
            if (Owner.GetRelic<MountainSee>() is { } mountain)
            {
                await mountain.PersistMaxHp1(mountain.DynamicVars.MaxHp.IntValue + gain);
                mountain.Flash();
            }
        }
        else if (target.GetPowerAmount<WolfSlot>() == 2)
        {
            if (Owner.GetRelic<TenguSee>() is { } tengu)
            {
                await tengu.PersistMaxHp2(tengu.DynamicVars["Max"].IntValue + gain);
                tengu.Flash();
            }
            if (Owner.GetRelic<MountainSee>() is { } mountain)
            {
                await mountain.PersistMaxHp2(mountain.DynamicVars["Max"].IntValue + gain);
                mountain.Flash();
            }
        }
    }

    // 移除椛本体与各白狼身上的「被瞄准」(AimedAt) 预判标记，保证交托重定向目标唯一
    private async Task ClearExistingAimedAt(PlayerChoiceContext choiceContext)
    {
        var creatures = new Creature?[]
        {
            Owner.Creature,
            WolfOne.Current?.Creature,
            WolfTwo.Current?.Creature,
        };
        foreach (var creature in creatures)
        {
            if (creature != null && creature.FindPower<AimedAt>() is { } power)
            {
                await PowerCmd.Remove(power);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxHpGain"].UpgradeValueBy(3);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.WhiteWolf),
        HoverTipFactory.FromPower<AimedAt>()
    ];
}
