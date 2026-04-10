using System;
using System.IO;
using System.Reflection;
using Cosmere.Core.Settings;
using HarmonyLib;
using Verse;

namespace Cosmere.Core;

internal static class CosmerePatchGuard {
    internal static bool Patched;
}

public abstract class CosmereMod<TSettings> : Verse.Mod where TSettings : CosmereModSettings, new() {
    public CosmereMod(ModContentPack content) : base(content) {
        LongEventHandler.QueueLongEvent(
            () => {
                if (!CosmerePatchGuard.Patched) {
                    Logger.Verbose(
                        $"{content.PackageId} Build Rev: {BuildInfo.Revision} @ {BuildInfo.BuildTime}"
                    );
                    Assembly assembly = GetType().Assembly;
                    string dllPath = assembly.Location;
                    DateTime lastWrite = File.GetLastWriteTime(dllPath);
                    Logger.Important($"Cosmere DLL compiled: {lastWrite:yyyy-MM-dd HH:mm:ss}");

                    new Harmony(content.PackageId).PatchAll(assembly);
                    CosmerePatchGuard.Patched = true;
                }
            },
            "LoadCosmereMod",
            true,
            null
        );
    }

    public static TSettings Settings => Mod.GetModSettings<TSettings>();
}