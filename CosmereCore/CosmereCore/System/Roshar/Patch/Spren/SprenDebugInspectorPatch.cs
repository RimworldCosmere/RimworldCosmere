using HarmonyLib;
using LudeonTK;
using Cosmere.System.Roshar.LesserSpren.MapComponent;
using Verse;

namespace Cosmere.System.Roshar.Patch.Spren;

[HarmonyPatch(typeof(EditWindow_DebugInspector), "CurrentDebugString")]
public static class SprenDebugInspectorPatch {
    public static void Postfix(ref string __result) {
        if (!Core.Mod.debugMode) return;

        LesserSprenSpawner? spawner = Find.CurrentMap?.GetComponent<LesserSprenSpawner>();
        if (spawner == null) return;

        __result += "\n" + spawner.DebugStringAt(Verse.UI.MouseCell());
    }
}