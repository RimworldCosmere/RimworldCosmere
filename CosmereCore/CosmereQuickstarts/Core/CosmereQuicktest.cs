using System;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Quest;
using Verse;

namespace Cosmere.Core.Quickstart;

/// <summary>
///     Wires Cosmere's Shards into the Quickstarts mod. Vanilla's quicktest hardcodes
///     Crashlanded, which carries no shard mod extension, so without this a quicktest map comes
///     up with nothing Invested working.
/// </summary>
[StaticConstructorOnStartup]
public static class CosmereQuicktest {
    /// <summary>
    ///     What the shard dialog starts on: enough to cover Scadrial, Roshar and Taldain at once,
    ///     since that is what a throwaway test map is usually for.
    /// </summary>
    public static readonly string[] DefaultShards = [
        "Ruin",
        "Preservation",
        "Odium",
        "Honor",
        "Cultivation",
        "Autonomy",
    ];

    private static IReadOnlyList<string> pending = [];

    static CosmereQuicktest() {
        VanillaQuicktest.RowAction = () => Find.WindowStack.Add(new Dialog_QuicktestShards());
        VanillaQuicktest.Configuring += ApplyPendingShards;
        CosmereQuestManager.eraProvider = ForcedEra;
    }

    /// <summary>Starts vanilla's quicktest with the given Shards enabled.</summary>
    public static void Start(IReadOnlyList<string> shards) {
        pending = shards;
        VanillaQuicktest.Start();
    }

    /// <summary>Turns on the named Shards in the current game. Does nothing for an empty list.</summary>
    public static void EnableShards(IReadOnlyList<string> shards) {
        if (shards.Count == 0) return;

        Shards? component = Current.Game?.GetComponent<Shards>();
        if (component == null) {
            Log.Warn("Quickstart shards skipped: the game has no Shards component yet.");
            return;
        }

        // Conflicts allowed: exercising everything at once is the point of a quicktest.
        for (int i = 0; i < shards.Count; i++) {
            component.EnableShard(shards[i], true);
        }

        Log.Debug($"Quickstart enabled shards: {string.Join(", ", component.enabledShards.Keys)}");
    }

    private static string? ForcedEra() {
        return (Quickstarter.Instance?.Quickstart as CosmereQuickstartBase)?.era;
    }

    private static void ApplyPendingShards() {
        EnableShards(pending);
    }
}
