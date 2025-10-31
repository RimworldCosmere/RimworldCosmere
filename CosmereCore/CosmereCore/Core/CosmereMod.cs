using Cosmere.Core.Settings;
using HarmonyLib;
using Verse;

namespace Cosmere.Core;

public abstract class CosmereMod<TSettings> : Verse.Mod where TSettings : CosmereModSettings, new() {
    public CosmereMod(ModContentPack content) : base(content) {
        LongEventHandler.QueueLongEvent(
            () => {
                Logger.Verbose(
                    $"{content.PackageId} Build Rev: {BuildInfo.Revision} @ {BuildInfo.BuildTime}"
                );

                new Harmony(content.PackageId).PatchAll(GetType().Assembly);
            },
            "LoadCosmereMod",
            true,
            null
        );
    }

    public static TSettings Settings => Mod.GetModSettings<TSettings>();
}