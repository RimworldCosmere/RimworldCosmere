using CosmereFramework;
using HarmonyLib;
using Verse;
using static CosmereFramework.CosmereFramework;
using Log = CosmereFramework.Log;

namespace CosmereCore {
    public class CosmereCore(ModContentPack content) : Mod(content) {
        public static CosmereCore CosmereCoreMod => LoadedModManager.GetMod<CosmereCore>();
    }

    [StaticConstructorOnStartup]
    public static class ModStartup {
        static ModStartup() {
            Log.Important($"Build Rev: {BuildInfo.REVISION} @ {BuildInfo.BUILD_TIME}. DebugMode={CosmereSettings.debugMode} LogLevel={CosmereSettings.logLevel}");
            new Harmony("Cryptiklemur.Cosmere.Core").PatchAll();
        }
    }
}