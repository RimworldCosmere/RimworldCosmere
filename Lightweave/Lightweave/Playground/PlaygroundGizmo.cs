using Verse;

namespace Cosmere.Lightweave.Playground;

public sealed class PlaygroundGizmo : Command_Action {
    public PlaygroundGizmo() {
        defaultLabel = "Lightweave Playground";
        defaultDesc = "Open the Lightweave primitive preview window.";
        action = () => Find.WindowStack.Add(new LightweavePlayground());
    }

    public static bool ShouldShow => Prefs.DevMode;
}