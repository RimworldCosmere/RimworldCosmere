using System.Collections.Generic;
using System.Linq;
using CosmereCore.ModExtensions;
using CosmereCore.Utils;
using CosmereFramework;
using HarmonyLib;
using RimWorld;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Patches {
    [HarmonyPatch(typeof(PageUtility), nameof(PageUtility.StitchedPages))]
    public static class LockShardSelection {
        private static void Prefix(ref IEnumerable<Page> pages) {
            var scenarioName = Find.Scenario?.name;
            var def = DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.label == scenarioName);
            var shards = def?.GetModExtension<Shards>();
            if (shards == null) return;

            if (shards.shards.Count == 0) {
                Log.Message($"[CosmereScadrial] No matching shard system found for {def.defName}", LogLevel.Warning);
                return;
            }

            foreach (var shard in shards.shards) {
                ShardUtility.Enable(shard);
            }

            Messages.Message(
                $"Shard System has been pre-selected: {string.Join(", ", ShardUtility.shards.enabledShardDefs.Select(x => x.label))}",
                MessageTypeDefOf.PositiveEvent);
        }
    }
}