using Concord;
using Cosmere.Core.Page;
using RimWorld;
using Verse;
using DefModExtension_Shards = Cosmere.Core.DefModExtension.Shards;

namespace Cosmere.Core.Patch;

[Patch(typeof(PageUtility))]
public static class InsertShardSelectionPatch {
    private static bool allowShardChange {
        get {
            string? scenarioName = Find.Scenario?.name;
            ScenarioDef? def =
                DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.label == scenarioName);
            DefModExtension_Shards? shards = def?.GetModExtension<DefModExtension_Shards>();

            return shards == null || shards.allowChange;
        }
    }

    [Inject(At.Head, nameof(PageUtility.StitchedPages))]
    private static void BeforeStitchedPages(IEnumerable<RimWorld.Page> pages) {
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
