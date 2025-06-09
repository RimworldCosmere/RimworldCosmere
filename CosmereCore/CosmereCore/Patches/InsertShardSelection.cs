using System.Collections.Generic;
using System.Linq;
using CosmereCore.Pages;
using HarmonyLib;
using RimWorld;
using Verse;
using Shards = CosmereCore.ModExtensions.Shards;

namespace CosmereCore.Patches {
    [HarmonyPatch(typeof(PageUtility), nameof(PageUtility.StitchedPages))]
    public static class InsertShardSelection {
        private static bool allowShardChange {
            get {
                var scenarioName = Find.Scenario?.name;
                var def = DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.label == scenarioName);
                var shards = def?.GetModExtension<Shards>();

                return shards == null || shards.allowChange;
            }
        }

        private static void Prefix(ref IEnumerable<Page> pages) {
            if (!allowShardChange) {
                return;
            }

            var list = pages.ToList();

            if (list.Any(p => p is SelectShards)) {
                return;
            }

            list.Insert(1, new SelectShards());
            pages = list;
        }
    }
}