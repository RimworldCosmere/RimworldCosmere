using System.Collections.Generic;
using System.Linq;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

/// <summary>
///     Holds a koloss to the four spikes that made it.
/// </summary>
/// <remarks>
///     A koloss is four spikes driven in at once, and the gene's own description says so - but
///     nothing recorded them, so a koloss from a raid or the dev menu had no spikes in its body at
///     all. Nothing to pull out, nothing for Ruin to speak through, and nothing for a surgeon to
///     find.
///     <para>
///         Shaped after <see cref="BlessingBound" />, which does the same job for a kandra's pair:
///         seed on add, then re-check slowly, because spikes come out through surgery rather than
///         between two ticks.
///     </para>
/// </remarks>
public class SpikeBound : Verse.Gene {
    /// <summary>Four, driven at once. Fewer and what is left is not a koloss.</summary>
    public const int SpikeCount = 4;

    /// <summary>
    ///     What the four are made of, which is iron, four times.
    /// </summary>
    /// <remarks>
    ///     Iron steals human strength. Four of them is why a koloss is enormous, and the absence of
    ///     any mental metal among them is why there is so little left inside it.
    /// </remarks>
    private const string Metal = "Iron";

    public override void PostAdd() {
        base.PostAdd();

        if (pawn.health?.hediffSet == null) return;

        // SetXenotype calls AddGene once per gene and nothing dedupes, so a second Become would
        // otherwise drive another four in.
        if (KolossUtility.SpikeCount(pawn) > 0) return;

        BodyPartRecord? core = pawn.RaceProps?.body?.corePart == null
            ? null
            : pawn.health.hediffSet.GetNotMissingParts()
                .FirstOrDefault(p => p.def == pawn.RaceProps.body.corePart.def);

        for (int i = 0; i < SpikeCount; i++) {
            HemalurgicImplantUtility.AddToUnifiedHediff(
                pawn,
                new ImplantedSpikeData { metalDefName = Metal, chargeStrength = 1f },
                core
            );
        }

        HemalurgicImplantUtility.UpdateRuinsInfluence(pawn);
    }

    /// <summary>
    ///     Slow on purpose. A spike leaves through surgery, which is an event.
    /// </summary>
    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;

        KolossUtility.ReconcileSpikes(pawn);
    }
}
