using Verse;

namespace Cosmere.Lightweave.Runtime;

public static class LightweaveLog {
    public static void Message(string text) => Log.Message($"[Lightweave] {text}");
    public static void Warning(string text) => Log.Warning($"[Lightweave] {text}");
    public static void Error(string text) => Log.Error($"[Lightweave] {text}");
}
