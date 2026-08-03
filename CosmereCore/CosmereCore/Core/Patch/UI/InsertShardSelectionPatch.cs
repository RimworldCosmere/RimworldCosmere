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

    // Spliced into the stitched chain on the way out rather than into the page list on the way in:
    // the target takes its pages by value, so reassigning the argument left the caller's original
    // sequence untouched and the page never appeared.
    [Inject(At.Return, nameof(PageUtility.StitchedPages))]
    private static void AfterStitchedPages(ControlHandle<RimWorld.Page> ch) {
        if (!allowShardChange) {
            return;
        }

        RimWorld.Page? first = ch.ReturnValue;
        if (first == null) {
            return;
        }

        for (RimWorld.Page? page = first; page != null; page = page.next) {
            if (page is SelectShards) return;
        }

        SelectShards inserted = new SelectShards();
        RimWorld.Page? second = first.next;

        first.next = inserted;
        inserted.prev = first;
        inserted.next = second;
        if (second != null) second.prev = inserted;
    }
}
