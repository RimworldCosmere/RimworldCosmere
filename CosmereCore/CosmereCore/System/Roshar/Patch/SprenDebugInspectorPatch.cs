using Cosmere.System.Roshar.LesserSpren.MapComponent;
using HarmonyLib;
using LudeonTK;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(EditWindow_DebugInspector), "CurrentDebugString")]
public static class SprenDebugInspectorPatch {
    public static void Postfix(ref string __result) {
        if (!Foundation.Mod.debugMode) return;

        LesserSprenSpawner? spawner = Find.CurrentMap?.GetComponent<LesserSprenSpawner>();
        if (spawner == null) return;

        __result += "\n" + spawner.DebugStringAt(UI.MouseCell());
    }
}