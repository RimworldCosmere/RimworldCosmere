using System;
using System.IO;
using System.Reflection;
using Concord;
using Cosmere.Core.Framework;
using Cosmere.Core.Settings;
using Verse;

namespace Cosmere.Core;

internal static class CosmerePatchGuard {
    internal static bool Patched;

    // Held for the process lifetime; disposing the handle would unpatch everything.
    internal static IPatchHandle? ConcordPatches;

    // One mod loads the patches for all of them, whichever one the game builds first.
    internal static void EnsurePatched(Assembly assembly, string packageId) {
        if (Patched) return;

        Log.Debug($"{packageId} Build Rev: {BuildInfo.Revision} @ {BuildInfo.BuildTime}");
        DateTime lastWrite = File.GetLastWriteTime(assembly.Location);
        Log.Info($"Cosmere DLL compiled: {lastWrite:yyyy-MM-dd HH:mm:ss}");

        ConcordPatches = Patcher.Apply(assembly);
        Patched = true;
    }
}

public abstract class CosmereMod<TSettings> : Verse.Mod
    where TSettings : CosmereModSettings, new() {
    protected CosmereMod(ModContentPack content) : base(content) {
        LongEventHandler.QueueLongEvent(
            () => CosmerePatchGuard.EnsurePatched(GetType().Assembly, content.PackageId),
            "LoadCosmereMod",
            true,
            null
        );
    }

    public static TSettings Settings => Mod.GetModSettings<TSettings>();
}
