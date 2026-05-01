using LudeonTK;
using Verse;

namespace Cosmere.Lightweave.Playground;

[StaticConstructorOnStartup]
public static class PlaygroundDebugAction {
    [DebugAction("Cosmere/Core", "Open Lightweave Playground", allowedGameStates = AllowedGameStates.Playing)]
    public static void Open() {
        Find.WindowStack.Add(new LightweavePlayground());
    }
}