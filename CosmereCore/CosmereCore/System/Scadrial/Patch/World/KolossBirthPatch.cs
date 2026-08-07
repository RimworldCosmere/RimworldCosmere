using Concord;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.World;

/// <summary>
///     Koloss parents have koloss-blooded children.
/// </summary>
/// <remarks>
///     The koloss xenotype is not inheritable, because a koloss is made rather than born and
///     nobody is born with four spikes in them. That leaves vanilla with nothing to pass down,
///     so the child would come out an ordinary baseliner and the bloodline Harmony left them
///     would not exist.
///     <para>
///         One koloss parent is enough. Two koloss is the case the fiction describes, but a
///         koloss and a human produce the same thing, and refusing that would leave a child
///         whose father is visibly a koloss looking like nobody's.
///     </para>
/// </remarks>
[Patch(typeof(PregnancyUtility))]
public static class KolossBirthPatch {
    [Inject(At.Return, nameof(PregnancyUtility.ApplyBirthOutcome))]
    private static void AfterApplyBirthOutcome(
        Pawn geneticMother,
        Pawn father,
        ControlHandle<Verse.Thing> ch
    ) {
        // Stillbirths come back as a corpse, and there is nothing to set on one.
        if (ch.ReturnValue is not Pawn child) return;
        if (child.genes == null) return;

        if (!KolossUtility.IsKoloss(geneticMother) && !KolossUtility.IsKoloss(father)) return;

        XenotypeDef? blooded = DefDatabase<XenotypeDef>.GetNamedSilentFail(
            KolossUtility.KolossBloodedXenotype
        );
        if (blooded == null) return;

        child.genes.SetXenotypeDirect(blooded);

        Cosmere.Core.Logger.Verbose(
            $"KolossBirthPatch: {child.LabelShort} born koloss-blooded."
        );
    }
}
