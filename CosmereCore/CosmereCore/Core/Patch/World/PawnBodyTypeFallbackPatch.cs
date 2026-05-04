using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType")]
public static class PawnBodyTypeFallbackPatch {
    public static void Postfix(Pawn pawn) {
        if (pawn?.story == null) return;
        if (pawn.story.bodyType != null) return;
        if (pawn.RaceProps == null || !pawn.RaceProps.Humanlike) return;

        pawn.story.bodyType = FallbackBodyTypeFor(pawn);
    }

    public static BodyTypeDef FallbackBodyTypeFor(Pawn pawn) {
        if (ModsConfig.BiotechActive && pawn.DevelopmentalStage.Juvenile()) {
            return pawn.DevelopmentalStage == DevelopmentalStage.Baby
                ? BodyTypeDefOf.Baby
                : BodyTypeDefOf.Child;
        }

        return pawn.gender == Gender.Female
            ? BodyTypeDefOf.Female
            : BodyTypeDefOf.Male;
    }
}