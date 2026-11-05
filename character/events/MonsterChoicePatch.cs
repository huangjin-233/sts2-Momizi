#nullable enable
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Momizi.character.relics;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.events;

/// <summary>
/// 小怪节点二选一：拦截 RunManager.CreateRoom。
/// 当玩家进入普通小怪节点且持有 ≥1 情报时，把小怪战斗替换为情报抉择事件（普通 vs 强化遭遇）。
/// 情报在玩家选择"强化遭遇"时才扣除。
/// </summary>
[HarmonyPatch(typeof(RunManager), "CreateRoom")]
public static class MonsterChoicePatch
{
    private static bool Prefix(RunManager __instance, RoomType roomType, MapPointType mapPointType,
        AbstractModel? model, ref AbstractRoom __result)
    {
        // 只处理普通小怪节点，且未指定具体遭遇
        if (roomType != RoomType.Monster || mapPointType != MapPointType.Monster || model != null)
        {
            return true;
        }

        RunState? state = AccessTools.Property(typeof(RunManager), "State")?.GetValue(__instance) as RunState;
        if (state == null)
        {
            return true;
        }

        // 一人选全队享：指定"领头"玩家 = 队伍中第一个持有 ≥{Tengu.IntelligenceCost} 情报的玩家；
        // 有人选"强化"时只扣他 3 情报，强化词条与战斗结果全队共享。
        Tengu? leader = state.Players
            .Select(p => p.GetRelic<Tengu>())
            .FirstOrDefault(t => t is { Intelligence: >= Tengu.IntelligenceCost });
        if (leader == null)
        {
            return true;
        }

        // 每次进入随机抽一档（1/2/3 等概率），再从该档词条池/抽选池各独立抽 1 个
        var tier = state.Rng.Niche.NextInt(3) + 1;
        var modifier = PickModifier(state, tier);
        var rewardPick = PickReward(state, tier);

        var choice = ModelDb.Event<IntelligenceChoiceEvent>();
        choice.CandidateA = null;
        choice.CandidateB = null;
        choice.FallbackEvent = null;
        choice.NormalEncounter = state.Act.PullNextEncounter(RoomType.Monster);
        choice.Tier = tier;
        choice.Modifier = modifier;
        choice.RewardPick = rewardPick;
        choice.DesignatedTengu = leader;
        choice.IsConfigured = true;

        __result = new EventRoom(choice);
        return false;
    }

    // 每档的词条池
    private static readonly IntelligenceChoiceEvent.MonsterModifier[][] ModifierPool =
    [
        // 占位（Tier=0 不用）
        [],
        // 一档
        [IntelligenceChoiceEvent.MonsterModifier.T1Block18, IntelligenceChoiceEvent.MonsterModifier.T1AllyVulnerable2,
         IntelligenceChoiceEvent.MonsterModifier.T1Vigor4, IntelligenceChoiceEvent.MonsterModifier.T1Hp18,
         IntelligenceChoiceEvent.MonsterModifier.T1Regen4],
        // 二档
        [IntelligenceChoiceEvent.MonsterModifier.T2Plating6, IntelligenceChoiceEvent.MonsterModifier.T2Block12Plating3,
         IntelligenceChoiceEvent.MonsterModifier.T2Strength2, IntelligenceChoiceEvent.MonsterModifier.T2DeathStrength3,
         IntelligenceChoiceEvent.MonsterModifier.T2Regen8, IntelligenceChoiceEvent.MonsterModifier.T2AllyWeak2,
         IntelligenceChoiceEvent.MonsterModifier.T2Thorns2, IntelligenceChoiceEvent.MonsterModifier.T2Artifact2,
         IntelligenceChoiceEvent.MonsterModifier.T2Hp26],
        // 三档
        [IntelligenceChoiceEvent.MonsterModifier.T3Slippery3, IntelligenceChoiceEvent.MonsterModifier.T3Str3Plating6,
         IntelligenceChoiceEvent.MonsterModifier.T3ExtraEnemy1, IntelligenceChoiceEvent.MonsterModifier.T3Hp56],
    ];

    // 每档的奖励抽选池
    private static readonly IntelligenceChoiceEvent.BoostedRewardOption[][] RewardPool =
    [
        [],
        // 一档
        [IntelligenceChoiceEvent.BoostedRewardOption.Potion1, IntelligenceChoiceEvent.BoostedRewardOption.Heal10,
         IntelligenceChoiceEvent.BoostedRewardOption.UpgradeRandom1, IntelligenceChoiceEvent.BoostedRewardOption.RetainRandom,
         IntelligenceChoiceEvent.BoostedRewardOption.GetCharCard],
        // 二档
        [IntelligenceChoiceEvent.BoostedRewardOption.UpgradeRandom1, IntelligenceChoiceEvent.BoostedRewardOption.RemoveCard,
         IntelligenceChoiceEvent.BoostedRewardOption.TransformCard, IntelligenceChoiceEvent.BoostedRewardOption.Potion2,
         IntelligenceChoiceEvent.BoostedRewardOption.MaxHp5],
        // 三档
        [IntelligenceChoiceEvent.BoostedRewardOption.RandomRelic, IntelligenceChoiceEvent.BoostedRewardOption.EnchantCard,
         IntelligenceChoiceEvent.BoostedRewardOption.UpgradeChoose1, IntelligenceChoiceEvent.BoostedRewardOption.UpgradeRandom2,
         IntelligenceChoiceEvent.BoostedRewardOption.RemoveCard],
    ];

    private static IntelligenceChoiceEvent.MonsterModifier PickModifier(RunState state, int tier)
    {
        var pool = ModifierPool[tier];
        return pool[state.Rng.Niche.NextInt(pool.Length)];
    }

    private static IntelligenceChoiceEvent.BoostedRewardOption PickReward(RunState state, int tier)
    {
        var pool = RewardPool[tier];
        return pool[state.Rng.Niche.NextInt(pool.Length)];
    }
}
