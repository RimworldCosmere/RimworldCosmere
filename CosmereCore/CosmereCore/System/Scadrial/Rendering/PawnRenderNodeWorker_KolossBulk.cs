using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Rendering;

/// <summary>
///     Draws a koloss at whatever size it has grown to.
/// </summary>
/// <remarks>
///     A koloss never stops growing, so its size is not a property of being a koloss - it is a
///     reading of how long this one has been alive. The growth hediff's severity is that reading,
///     and this worker turns it into a scale every frame.
///     <para>
///         A fixed drawSize in the gene def would have made a newly spiked koloss the same size
///         as a twenty-year-old one, which is the opposite of the thing the whole design is
///         about.
///     </para>
/// </remarks>
public class PawnRenderNodeWorker_KolossBulk : PawnRenderNodeWorker_Body {
    /// <summary>Size the moment the spikes go in. Barely more than the man they were.</summary>
    private const float NewlyMade = 1.05f;

    /// <summary>Size when the skin finally fails. Twice a man across the shoulders.</summary>
    private const float FullyGrown = 1.75f;

    public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms) {
        Vector3 scale = base.ScaleFor(node, parms);

        Pawn? pawn = parms.pawn;
        if (pawn == null) return scale;

        Hediff? growth = pawn.health?.hediffSet?.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth
        );
        if (growth == null) return scale;

        // Height only tracks width here. A koloss gets thicker as much as it gets taller, and
        // scaling the two apart made it read as a stretched human rather than a bigger thing.
        float grown = Mathf.Lerp(NewlyMade, FullyGrown, Mathf.Clamp01(growth.Severity));

        return scale * grown;
    }
}
