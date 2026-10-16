using Concord;
using Cosmere.System.Roshar.LesserSpren.MapComponent;
using LudeonTK;
using Verse;

namespace Cosmere.System.Roshar.Patch.Spren;

[Patch]
public abstract class SprenDebugInspectorPatch : EditWindow_DebugInspector {
    [Inject(At.Return, "CurrentDebugString")]
    private static void AfterCurrentDebugString(ControlHandle<string> ch) {
        if (!Core.Mod.DebugMode) return;

        LesserSprenSpawner? spawner = Find.CurrentMap?.GetComponent<LesserSprenSpawner>();
        if (spawner == null) return;

        ch.ReturnValue += "\n" + spawner.DebugStringAt(Verse.UI.MouseCell());
    }
}
