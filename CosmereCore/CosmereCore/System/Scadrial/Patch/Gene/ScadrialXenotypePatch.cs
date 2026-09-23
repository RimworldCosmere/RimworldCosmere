using Concord;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Gene;

[Patch(typeof(Verse.PawnGenerator))]
public static class ScadrialXenotypePatch {
    /// <summary>
    ///     return injection, not head: cancelling the original broke generation for a factionless pawn;
    ///     overriding the answer is enough. priority 1 composes outermost, matching the old harmony postfix.
    /// </summary>
    [Inject(At.Return, nameof(Verse.PawnGenerator.GetXenotypeForGeneratedPawn), Priority = 1)]
    private static void AfterGetXenotypeForGeneratedPawn(
        PawnGenerationRequest request,
        ControlHandle<XenotypeDef> ch
    ) {
        if (request.ForcedXenotype != null) return;

        // only humanlikes with gene trackers get a xenotype; forcing one on Anomalys monolith breaks generation
        if (request.KindDef?.RaceProps?.Humanlike != true) return;

        // not IsActive: cross-world sentinel has every world active at once; a self-naming faction outranks the planet
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

        // weighted, not even: koloss-blooded thinned the line, didnt replace it (gene desc: spread, not took over)
        ch.ReturnValue = new (XenotypeDef xenotype, float weight)[] {
            (XenotypeDefOf.Cosmere_Scadrial_Xenotype_Scadrian, 60f),
            (XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris, 35f),
            (XenotypeDefOf.Cosmere_Scadrial_Xenotype_KolossBlooded, 5f),
        }.RandomElementByWeight(entry => entry.weight).xenotype;
    }
}
