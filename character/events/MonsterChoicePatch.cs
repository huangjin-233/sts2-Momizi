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

        // 一人选全队享：指定“领头”玩家 = 队伍中第一个持有≥1 情报的玩家；
        // 有人选“强化”时只扣他 1 情报，强化词条与战斗结果全队共享。
        Tengu? leader = state.Players
            .Select(p => p.GetRelic<Tengu>())
            .FirstOrDefault(t => t is { Intelligence: >= 1 });
        if (leader == null)
        {
            return true;
        }

        // 随机 1 个强化词条（用 run 的 RNG 保持可复现）
        var modifier = (IntelligenceChoiceEvent.MonsterModifier)(state.Rng.Niche.NextInt(3) + 1);

        var choice = ModelDb.Event<IntelligenceChoiceEvent>();
        choice.CandidateA = null;
        choice.CandidateB = null;
        choice.FallbackEvent = null;
        choice.NormalEncounter = state.Act.PullNextEncounter(RoomType.Monster);
        choice.Modifier = modifier;
        choice.DesignatedTengu = leader;
        choice.IsConfigured = true;

        __result = new EventRoom(choice);
        return false;
    }
}
