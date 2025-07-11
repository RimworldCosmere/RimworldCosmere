using System.Collections.Generic;
using System.Linq;
using CosmereCore.DefModExtension;
using CosmereCore.Util;
using CosmereFramework;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patch;

[HarmonyPatch(typeof(PageUtility), nameof(PageUtility.StitchedPages))]
public static class LockShardSelection {
    private static void Prefix(ref IEnumerable<Page> pages) {
        string? scenarioName = Find.Scenario?.name;
        ScenarioDef? def = DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.label == scenarioName);
        Shards? shards = def?.GetModExtension<Shards>();
        if (shards == null) return;

        if (shards.shards.Count == 0) {
            Logger.Message($"[CosmereScadrial] No matching shard system found for {def!.defName}", LogLevel.Warning);
            return;
        }

        foreach (string? shard in shards.shards) {
            ShardUtility.Enable(shard);
        }

        Messages.Message(
            "CS_LockShardSelection".Translate(
                string.Join(", ", ShardUtility.shards.enabledShardDefs.Select(x => x.label)).Named("SHARDS")
            ),
            MessageTypeDefOf.NeutralEvent
        );
    }
}