using CosmereFramework;
using HarmonyLib;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereCore {
    public class CosmereCore(ModContentPack content) : Mod(content);

    [StaticConstructorOnStartup]
    public static class ModStartup {
        static ModStartup() {
            Log.Important($"Build Rev: {BuildInfo.REVISION} @ {BuildInfo.BUILD_TIME}. DebugMode={cosmereSettings.debugMode} LogLevel={cosmereSettings.logLevel}");
            new Harmony("cryptiklemur.cosmere.core").PatchAll();
            Log.Verbose("Harmony patches applied.");
        }

        private static CosmereSettings cosmereSettings => LoadedModManager.GetMod<CosmereFramework.CosmereFramework>().cosmereSettings;
    }
}