using Cosmere.System.Roshar.Surgebinding;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.Destroy))]
public static class ArtDestructionViolationPatch {
    private static void Prefix(Verse.Thing __instance, DestroyMode mode) {
        if (mode != DestroyMode.Deconstruct) return;

        CompArt compArt = __instance.TryGetComp<CompArt>();
        if (compArt == null) return;
        if (!compArt.Active) return;

        Map map = __instance.Map;
        if (map == null) return;

        List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (ViolationUtility.IsSurgebinderOfOrder(colonist, "Lightweaver")) {
                ViolationUtility.ApplyViolation(colonist, 0.1f, "deconstructing artwork");
            }
        }
    }
}
