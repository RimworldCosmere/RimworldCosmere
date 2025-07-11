using System.Diagnostics;
using HarmonyLib;

namespace CosmereFramework;

public static class Startup {
    public static void Initialize(string harmonyId) {
        Logger.Verbose(
            $"Mod: {harmonyId} Build Rev: {BuildInfo.Revision} @ {BuildInfo.BuildTime}. DebugMode={CosmereFramework.debugMode} LogLevel={CosmereFramework.logLevel}"
        );

        new Harmony(harmonyId).PatchAll(new StackTrace().GetFrame(1)!.GetMethod()!.ReflectedType!.Assembly);
    }
}