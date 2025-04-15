using CosmereFramework;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereCore {
    public class CosmereCore : Mod {
        public CosmereCore(ModContentPack content) : base(content) {
            Log.Important($"Build Rev: {BuildInfo.Revision} @ {BuildInfo.BuildTime}. DebugMode={CosmereSettings.debugMode} LogLevel={CosmereSettings.logLevel}");
            if (!CosmereSettings.debugMode) return;
            LongEventHandler.QueueLongEvent(Verse.Log.TryOpenLogWindow, "CosmereCoreInit", false, null);
        }

        private static CosmereSettings CosmereSettings => LoadedModManager.GetMod<CosmereFramework.CosmereFramework>().Settings;
    }
}