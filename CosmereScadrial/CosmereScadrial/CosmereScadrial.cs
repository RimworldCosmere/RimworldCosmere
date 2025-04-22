using CosmereCore.Tabs;
using CosmereFramework;
using CosmereScadrial.Inspector;
using HarmonyLib;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial {
    public class CosmereScadrial : Mod {
        public CosmereScadrial(ModContentPack content) : base(content) {
            InvestitureTabRegistry.Register(InvestitureTabScadrial.Draw);
        }
    }

    [StaticConstructorOnStartup]
    public static class ModStartup {
        static ModStartup() {
            Log.Important($"Build Rev: {BuildInfo.REVISION} @ {BuildInfo.BUILD_TIME}. DebugMode={cosmereSettings.debugMode} LogLevel={cosmereSettings.logLevel}");
            new Harmony("cryptiklemur.cosmere.scadrial").PatchAll();
            Log.Verbose("Harmony patches applied.");
        }

        private static CosmereSettings cosmereSettings => LoadedModManager.GetMod<CosmereFramework.CosmereFramework>().cosmereSettings;
    }
}