using System.Linq;
using CosmereCore.Utils;
using HarmonyLib;
using RimWorld;
using Verse;
using Log = CosmereFramework.Log;
using Shards = CosmereCore.ModExtensions.Shards;

namespace CosmereCore.Patches {
    [HarmonyPatch(typeof(Scenario), nameof(Scenario.PreConfigure))]
    public static class PreSelectShardForScenario {
        private static void Prefix() {
            var scenarioName = Find.Scenario?.name;
            if (string.IsNullOrEmpty(scenarioName)) return;

            var def = DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.label == scenarioName);
            var shards = def?.GetModExtension<Shards>();
            if (shards == null) return;

            if (shards.shards.Count == 0) {
                Log.Message($"[CosmereCore] No matching shard system found for {def.defName}");
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