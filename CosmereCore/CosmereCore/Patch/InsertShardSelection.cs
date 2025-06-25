using System.Collections.Generic;
using System.Linq;
using CosmereCore.Page;
using HarmonyLib;
using RimWorld;
using Verse;
using Shards = CosmereCore.DefModExtension.Shards;

namespace CosmereCore.Patch;

[HarmonyPatch(typeof(PageUtility), nameof(PageUtility.StitchedPages))]
public static class InsertShardSelection {
    private static bool allowShardChange {
        get {
            string? scenarioName = Find.Scenario?.name;
            ScenarioDef? def =
                DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.label == scenarioName);
            Shards? shards = def?.GetModExtension<Shards>();

            return shards == null || shards.allowChange;
        }
    }

    private static void Prefix(ref IEnumerable<RimWorld.Page> pages) {
        if (!allowShardChange) {
            return;
        }

        List<RimWorld.Page> list = pages.ToList();

        if (list.Any(p => p is SelectShards)) {
            return;
        }

        list.Insert(1, new SelectShards());
        pages = list;
    }
}