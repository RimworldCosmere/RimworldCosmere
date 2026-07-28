using Concord;
using Cosmere.Core.Util;
using RimWorld;
using Verse;
using DefModExtension_Shards = Cosmere.Core.DefModExtension.Shards;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class PreSelectShardForScenarioPatch : Scenario {
    [Inject(At.Head, nameof(PreConfigure))]
    private void BeforePreConfigure() {
        string? scenarioName = Find.Scenario?.name;
        if (string.IsNullOrEmpty(scenarioName)) return;

        ScenarioDef? def = DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.label == scenarioName);
        DefModExtension_Shards? shards = def?.GetModExtension<DefModExtension_Shards>();
        if (shards?.shards == null) return;

        if (shards.shards.Count == 0) {
            Logger.Message($"[Core] No matching shard system found for {def?.defName}");
            return;
        }

        foreach (string? shard in shards.shards) {
            ShardUtility.Enable(shard);
        }
    }
}
