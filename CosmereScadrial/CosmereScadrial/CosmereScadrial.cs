using CosmereCore.Tabs;
using CosmereFramework;
using CosmereScadrial.Inspector;
using HarmonyLib;
using Verse;
using static CosmereFramework.CosmereFramework;
using Log = CosmereFramework.Log;

namespace CosmereScadrial {
    public class CosmereScadrial : Mod {
        public CosmereScadrial(ModContentPack content) : base(content) {
            InvestitureTabRegistry.Register(InvestitureTabScadrial.Draw);
        }

        public static CosmereScadrial CosmereScadrialMod => LoadedModManager.GetMod<CosmereScadrial>();
    }

    [StaticConstructorOnStartup]
    public static class ModStartup {
        static ModStartup() {
            Log.Important($"Build Rev: {BuildInfo.REVISION} @ {BuildInfo.BUILD_TIME}. DebugMode={CosmereSettings.debugMode} LogLevel={CosmereSettings.logLevel}");
            new Harmony("cryptiklemur.cosmere.scadrial").PatchAll();
        }
    }
}