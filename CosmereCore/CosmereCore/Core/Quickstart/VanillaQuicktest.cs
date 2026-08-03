using System.Reflection;
using Cosmere.Core.Comp.Game;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quickstart;

/// <summary>
///     Runs RimWorld's own quicktest setup, with a shard set applied before the map generates.
///     Vanilla hardcodes Crashlanded, which carries no shard mod extension, so without this the
///     quicktest map comes up with nothing Invested working.
/// </summary>
public static class VanillaQuicktest {
    public static readonly string[] DefaultShards = [
        "Ruin",
        "Preservation",
        "Odium",
        "Honor",
        "Cultivation",
        "Autonomy",
    ];

    private static bool pickerPending;

    /// <summary>
    ///     Claims the -quicktest arg so the picker can be shown on the main menu instead of the
    ///     game dropping straight into a map.
    /// </summary>
    /// <remarks>
    ///     Done by flipping QuickStarter's own guard rather than by patching CheckQuickStart:
    ///     UIRoot_Entry.Init runs in the InitializingInterface long event, which Root.Start queues
    ///     before the mod constructor ever queues the event that applies this mod's patches. A
    ///     patch on that method would be composed several events too late to matter.
    /// </remarks>
    public static void ClaimCommandLineArg() {
        if (!GenCommandLine.CommandLineArgPassed("quicktest")) return;

        FieldInfo? guard = typeof(QuickStarter).GetField(
            "quickStarted",
            BindingFlags.Static | BindingFlags.NonPublic
        );
        if (guard == null) {
            Logger.Warning("QuickStarter.quickStarted is gone; -quicktest will load vanilla's map, not the picker.");
            return;
        }

        guard.SetValue(null, true);
        pickerPending = true;
    }

    /// <summary>
    ///     Opens the picker on the first main menu frame after the arg was claimed. Deferred this
    ///     far because the window stack does not exist yet when a static constructor runs.
    /// </summary>
    public static void ShowPickerIfPending() {
        if (!pickerPending || Find.WindowStack == null) return;

        pickerPending = false;
        Find.WindowStack.Add(new Dialog_QuicktestPicker());
    }

    public static void Start(IReadOnlyList<string> shards) {
        LongEventHandler.QueueLongEvent(
            () => {
                Root_Play.SetupForQuickTestPlay();
                EnableShards(shards);
                PageUtility.InitGameStart();
            },
            "GeneratingMap",
            true,
            GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap
        );
    }

    private static void EnableShards(IReadOnlyList<string> shards) {
        Shards? component = Current.Game?.GetComponent<Shards>();
        if (component == null) {
            Logger.Warning("Vanilla quicktest shards skipped: the game has no Shards component yet.");
            return;
        }

        // Conflicts allowed: the point of the quicktest is exercising everything at once, so
        // Ruin and Preservation both being on is the intent rather than an accident.
        for (int i = 0; i < shards.Count; i++) {
            component.EnableShard(shards[i], true);
        }

        Logger.Verbose($"Vanilla quicktest enabled shards: {string.Join(", ", component.enabledShards.Keys)}");
    }
}
