using Cosmere.Core.DefModExtension;
using HarmonyLib;
using Verse;
using Cosmere.Core.Util;
using RimWorld;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(PageUtility), nameof(PageUtility.StitchedPages))]
public static class LockShardSelectionPatch {
    private static void Prefix(ref IEnumerable<RimWorld.Page> pages) {
        string? scenarioName = Find.Scenario?.name;
        ScenarioDef? def = DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.label == scenarioName);
        Shards? shards = def?.GetModExtension<Shards>();
        if (shards?.shards == null) return;

        if (shards.shards.Count == 0) {
            Logger.Message($"No matching shard system found for {def!.defName}", LogLevel.Warning);
            return;
        }

        foreach (string? shard in shards.shards) {
            ShardUtility.Enable(shard);
        }

        Comp.Game.Shards? gameShards = ShardUtility.shards;
        if (gameShards == null) return;

        Messages.Message(
            "CS_LockShardSelection".Translate(
                string.Join(", ", gameShards.enabledShards.Values.Select(x => x.Label)).Named("SHARDS")
            ),
            MessageTypeDefOf.NeutralEvent
        );
    }
}