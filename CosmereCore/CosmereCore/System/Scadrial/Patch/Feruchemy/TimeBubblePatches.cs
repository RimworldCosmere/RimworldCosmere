using Concord;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Feruchemy;

[Patch]
public abstract class TimeBubblePatch : Pawn_NeedsTracker {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected TimeBubblePatch(Pawn newPawn) : base(newPawn) { }

    [Inject(At.Head, nameof(NeedsTrackerTickInterval))]
    private Control BeforeNeedsTrackerTickInterval(int delta) {
        const int BaseInterval = 150;
        const int CadmiumMultiplier = 3;
        const int BendalloyDivisor = 3;

        Pawn? pawn = trackedPawn;
        if (pawn?.health == null || pawn.Dead) return Control.Continue;

        // If we are in a cadmium bubble, time slows down, needs should decay a third as fast
        if (pawn.health.hediffSet.HasHediff(HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleCadmium)) {
            if (!pawn.IsHashIntervalTick(BaseInterval * CadmiumMultiplier, delta)) {
                return Control.Cancel;
            }
        } else if (pawn.health.hediffSet.HasHediff(HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleBendalloy)) {
            if (!pawn.IsHashIntervalTick(Mathf.RoundToInt((float)BaseInterval / BendalloyDivisor), delta)) {
                return Control.Cancel;
            }
        } else {
            return Control.Continue;
        }

        Pawn_NeedsTracker self = this;
        for (int index = 0; index < self.AllNeeds.Count; ++index) {
            self.AllNeeds[index].NeedInterval();
        }

        return Control.Cancel;
    }
}
