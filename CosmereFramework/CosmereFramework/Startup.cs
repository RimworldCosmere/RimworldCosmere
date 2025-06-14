using System.Diagnostics;
using HarmonyLib;

namespace CosmereFramework {
    public static class Startup {
        public static void Initialize(string harmonyId) {
            Log.Important(
                $"Mod: {harmonyId} Build Rev: {BuildInfo.REVISION} @ {BuildInfo.BUILD_TIME}. DebugMode={CosmereFramework.CosmereSettings.debugMode} LogLevel={CosmereFramework.CosmereSettings.logLevel}");

            new Harmony(harmonyId).PatchAll(new StackTrace().GetFrame(1).GetMethod().ReflectedType.Assembly);
        }
    }
}