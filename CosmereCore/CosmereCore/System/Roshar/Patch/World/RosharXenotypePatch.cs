using Concord;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.World;

[Patch(typeof(Verse.PawnGenerator))]
public static class RosharXenotypePatch {
    /// <summary>
    ///     A return injection, not a head one: cancelling the original left vanilla generation failing
    ///     on a factionless pawn. Overriding the answer is enough.
    /// </summary>
    /// <remarks>
    ///     Priority 1 so this composes outermost, matching the Harmony Priority.Low postfix it replaces.
    /// </remarks>
    [Inject(At.Return, nameof(Verse.PawnGenerator.GetXenotypeForGeneratedPawn), Priority = 1)]
    private static void AfterGetXenotypeForGeneratedPawn(
        PawnGenerationRequest request,
        ControlHandle<XenotypeDef> ch
    ) {
        if (request.ForcedXenotype != null) return;

        // xenotypes only mean something to humanlikes; forcing one onto Anomaly's monolith breaks generation
        if (request.KindDef?.RaceProps?.Humanlike != true) return;

        // not IsActive: the cross-world sentinel has every world active at once; a self-naming faction wins
        if (XenotypeArbiter.FactionSpeaksForItself(request)) return;

        if (!XenotypeArbiter.MayAnswer(WorldDefOf.Roshar)) return;

        // 70% darkeyes, 30% lighteyes - reflecting Rosharan demographics
        ch.ReturnValue = Rand.Value < 0.7f
            ? XenotypeDefOf.Cosmere_Roshar_Xenotype_Darkeyes
            : XenotypeDefOf.Cosmere_Roshar_Xenotype_Lighteyes;
    }
}
