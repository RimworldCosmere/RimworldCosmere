using Cosmere.Framework.Settings;
using HarmonyLib;
using Verse;

namespace Cosmere.Framework;

public abstract class CosmereMod<TSettings> : Verse.Mod where TSettings : CosmereModSettings, new() {
    public CosmereMod(ModContentPack content) : base(content) {
        LongEventHandler.QueueLongEvent(
            () => {
                Logger.Verbose(
                    $"{content.PackageId} Build Rev: {BuildInfo.Revision} @ {BuildInfo.BuildTime}. DebugMode={Mod.debugMode} LogLevel={Mod.logLevel} Assembly={GetType().Assembly.GetName().FullName}"
                );

                Harmony.DEBUG = true;
                new Harmony(content.PackageId).PatchAll(GetType().Assembly);
            },
            "LoadCosmereMod",
            true,
            null
        );
    }

    public static TSettings Settings => Mod.GetModSettings<TSettings>();
}