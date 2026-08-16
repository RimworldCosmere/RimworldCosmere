using Concord;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Gene;

[Patch(typeof(Verse.PawnGenerator))]
public static class ScadrialXenotypePatch {
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

        // Not IsActive: on the cross-world sentinel every world is active at once, and two
        // patches writing the same return value let composition order pick the xenotype.
        // A faction that names its own people outranks the planet they stand on.
        if (XenotypeArbiter.FactionSpeaksForItself(request)) return;

        if (!XenotypeArbiter.MayAnswer(WorldDefOf.Scadrial)) return;

        bool preCatacendre = ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Preservation);

        if (preCatacendre) {
            ch.ReturnValue = new[] {
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Skaa,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Noble,
            }.RandomElement();
            return;
        }

        // Weighted rather than an even pick, because koloss-blooded are a thinning of the line
        // rather than a people. The gene's own description says their blood spread in the
        // generations after Harmony - spread, not took over.
        ch.ReturnValue = new (XenotypeDef xenotype, float weight)[] {
            (XenotypeDefOf.Cosmere_Scadrial_Xenotype_Scadrian, 60f),
            (XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris, 35f),
            (XenotypeDefOf.Cosmere_Scadrial_Xenotype_KolossBlooded, 5f),
        }.RandomElementByWeight(entry => entry.weight).xenotype;
    }
}
