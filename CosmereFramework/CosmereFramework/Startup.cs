namespace CosmereFramework {
    public static class Startup {
        public static void Initialize(string harmonyId) {
            Log.Important($"Build Rev: {BuildInfo.REVISION} @ {BuildInfo.BUILD_TIME}. DebugMode={CosmereFramework.CosmereSettings.debugMode} LogLevel={CosmereFramework.CosmereSettings.logLevel}");
            new HarmonyLib.Harmony(harmonyId).PatchAll();
        }
    }
}
