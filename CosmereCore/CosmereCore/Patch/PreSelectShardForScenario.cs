using System.Linq;
using CosmereCore.Util;
using HarmonyLib;
using RimWorld;
using Verse;
using Log = CosmereFramework.Log;
using Shards = CosmereCore.DefModExtension.Shards;

namespace CosmereCore.Patch;

[HarmonyPatch(typeof(Scenario), nameof(Scenario.PreConfigure))]
public static class PreSelectShardForScenario {
    private static void Prefix() {
        string? scenarioName = Find.Scenario?.name;
        if (string.IsNullOrEmpty(scenarioName)) return;

        ScenarioDef? def = DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.label == scenarioName);
        Shards? shards = def?.GetModExtension<Shards>();
        if (shards == null) return;

        if (shards.shards.Count == 0) {
            Log.Message($"[CosmereCore] No matching shard system found for {def?.defName}");
            return;
        }

        foreach (string? shard in shards.shards) {
            ShardUtility.Enable(shard);
        }

        Messages.Message(
            $"Shard System has been pre-selected: {string.Join(", ", (string[])ShardUtility.shards.enabledShardDefs.Select(x => x.label))}",
            MessageTypeDefOf.PositiveEvent);
    }
}