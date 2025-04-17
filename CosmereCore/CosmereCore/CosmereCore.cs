using CosmereFramework;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereCore {
    public class CosmereCore : Mod {
        public CosmereCore(ModContentPack content) : base(content) {
            Log.Important($"Build Rev: {BuildInfo.REVISION} @ {BuildInfo.BUILD_TIME}. DebugMode={cosmereSettings.debugMode} LogLevel={cosmereSettings.logLevel}");
            if (!cosmereSettings.debugMode) return;
            LongEventHandler.QueueLongEvent(Verse.Log.TryOpenLogWindow, "CosmereCoreInit", false, null);
        }

        public static CosmereSettings cosmereSettings => LoadedModManager.GetMod<CosmereFramework.CosmereFramework>().settings;
    }
}