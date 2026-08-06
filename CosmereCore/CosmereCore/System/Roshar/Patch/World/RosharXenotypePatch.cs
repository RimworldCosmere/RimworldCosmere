using Concord;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.World;

[Patch(typeof(Verse.PawnGenerator))]
public static class RosharXenotypePatch {
    // A return injection rather than a head one: cancelling the original stopped whatever else
    // it does on the way to a return value, which left vanilla generation to fail
    // on a factionless pawn. Overriding its answer is enough.
    //
    // Priority 1 so this composes outermost and its answer is the one that survives, matching
    // the Harmony Priority.Low postfix it replaces.
    [Inject(At.Return, nameof(Verse.PawnGenerator.GetXenotypeForGeneratedPawn), Priority = 1)]
    private static void AfterGetXenotypeForGeneratedPawn(
        PawnGenerationRequest request,
        ControlHandle<XenotypeDef> ch
    ) {
        if (request.ForcedXenotype != null) return;

        // A xenotype only means anything to a humanlike with a gene tracker.
        // Forcing one onto whatever else a map generator asks for - Anomaly's
        // monolith among them - breaks generation further down.
        if (request.KindDef?.RaceProps?.Humanlike != true) return;

        if (!WorldUtility.IsActive(WorldDefOf.Roshar)) return;

        // 70% darkeyes, 30% lighteyes - reflecting Rosharan demographics
        ch.ReturnValue = Rand.Value < 0.7f
            ? XenotypeDefOf.Cosmere_Roshar_Xenotype_Darkeyes
            : XenotypeDefOf.Cosmere_Roshar_Xenotype_Lighteyes;
    }
}
