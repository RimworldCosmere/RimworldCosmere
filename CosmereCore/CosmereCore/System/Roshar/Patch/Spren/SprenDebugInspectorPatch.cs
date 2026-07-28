using Concord;
using Cosmere.System.Roshar.LesserSpren.MapComponent;
using LudeonTK;
using Verse;

namespace Cosmere.System.Roshar.Patch.Spren;

[Patch]
public abstract class SprenDebugInspectorPatch : EditWindow_DebugInspector {
    [Inject(At.Return, "CurrentDebugString")]
    private void AfterCurrentDebugString(ControlHandle<string> ch) {
        if (!Core.Mod.debugMode) return;

        LesserSprenSpawner? spawner = Find.CurrentMap?.GetComponent<LesserSprenSpawner>();
        if (spawner == null) return;

        ch.ReturnValue += "\n" + spawner.DebugStringAt(Verse.UI.MouseCell());
    }
}
