using Cosmere.Core.UI.Radial;
using HarmonyLib;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
public static class RadialGizmoPatch {
    private static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo>? values, Pawn __instance) {
        if (values != null) {
            foreach (Gizmo gizmo in values) {
                yield return gizmo;
            }
        }

        if (__instance.Faction is not { IsPlayer: true }) yield break;
        if (!RadialController.HasRadialFor(__instance)) yield break;

        yield return new Command_OpenRadial(__instance);
    }
}
