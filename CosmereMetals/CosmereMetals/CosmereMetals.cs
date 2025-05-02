using CosmereFramework;
using HarmonyLib;
using Verse;
using static CosmereFramework.CosmereFramework;
using Log = CosmereFramework.Log;

namespace CosmereMetals {
    public class CosmereMetals(ModContentPack content) : Mod(content) {
        public static CosmereMetals CosmereMetalsMod => LoadedModManager.GetMod<CosmereMetals>();
    }

    [StaticConstructorOnStartup]
    public static class ModStartup {
        static ModStartup() {
            Log.Important($"Build Rev: {BuildInfo.REVISION} @ {BuildInfo.BUILD_TIME}. DebugMode={CosmereSettings.debugMode} LogLevel={CosmereSettings.logLevel}");
            new Harmony("Cryptiklemur.Cosmere.Metals").PatchAll();
        }
    }
}