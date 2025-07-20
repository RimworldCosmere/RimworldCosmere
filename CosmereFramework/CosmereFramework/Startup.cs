using System.Diagnostics;
using HarmonyLib;

namespace Cosmere.Framework;

public static class Startup {
    public static void Initialize(string harmonyId) {
        Logger.Verbose(
            $"Mod: {harmonyId} Build Rev: {BuildInfo.Revision} @ {BuildInfo.BuildTime}. DebugMode={Mod.debugMode} LogLevel={Mod.logLevel}"
        );

        new Harmony(harmonyId).PatchAll(new StackTrace().GetFrame(1)!.GetMethod()!.ReflectedType!.Assembly);
    }
}