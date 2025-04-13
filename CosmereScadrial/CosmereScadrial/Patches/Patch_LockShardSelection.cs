using System.Collections.Generic;
using System.Linq;
using CosmereCore.Utils;
using CosmereFramework;
using HarmonyLib;
using RimWorld;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereCore.Patches {
    [HarmonyPatch(typeof(PageUtility), nameof(PageUtility.StitchedPages))]
    public static class Patch_LockShardSelection {
        private static void Prefix(ref IEnumerable<Page> pages) {
            var scenarioName = Find.Scenario?.name;
            var defName = DefDatabase<ScenarioDef>.AllDefsListForReading.First(x => x.label == scenarioName)?.defName;
            if (defName != null && !defName.StartsWith("Cosmere_Scadrial_")) return;

            switch (defName) {
                case "Cosmere_Scadrial_PreCatacendre":
                    ShardUtility.Enable("Ruin");
                    ShardUtility.Enable("Preservation");
                    break;
                case "Cosmere_Scadrial_PostCatacendre":
                    ShardUtility.Enable("Harmony");
                    break;
                default:
                    Log.Message($"[CosmereScadrial] No matching shard system found for {defName}", LogLevel.Warning);
                    return;
            }

            Messages.Message(
                $"Shard System has been pre-selected: {string.Join(", ", ShardUtility.Shards.enabledShardDefs.Select(x => x.label))}",
                MessageTypeDefOf.PositiveEvent);
        }
    }
}