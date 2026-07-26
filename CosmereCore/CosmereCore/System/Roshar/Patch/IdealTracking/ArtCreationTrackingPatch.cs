using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(CompArt), nameof(CompArt.JustCreatedBy))]
public static class ArtCreationTrackingPatch {
    private static void Postfix(CompArt __instance, Pawn pawn) {
        if (pawn == null) return;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        CompQuality compQuality = __instance.parent.TryGetComp<CompQuality>();
        QualityCategory quality = compQuality != null ? compQuality.Quality : QualityCategory.Normal;

        surgebinder.OnArtCreated(quality);
        surgebinder.NotifyCreativeOutput();
    }
}