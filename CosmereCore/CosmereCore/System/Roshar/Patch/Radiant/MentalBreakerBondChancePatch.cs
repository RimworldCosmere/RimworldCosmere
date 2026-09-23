using Concord;
using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Radiant;

[Patch]
public abstract class MentalBreakerBondChancePatch : MentalBreaker {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected MentalBreakerBondChancePatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(MentalBreakerTickInterval))]
    private void AfterMentalBreakerTickInterval(int delta) {
        if (!GenTicks.IsTickIntervalDelta(GenTicks.TicksPerRealSecond, delta)) return;
        Pawn? pawn = trackedPawn;
        if (pawn.NonHumanlikeOrWildMan() || !pawn.IsColonist) return;
        PawnTracker pawnTracker = pawn.GetComp<PawnTracker>();
        if (pawnTracker == null || pawn.records.GetAsInt(RecordDefOf.Cosmere_Roshar_Record_BondsFormed) > 0) return;

        float increment = 0f;
        if (BreakExtremeIsImminent) {
            increment = 2.5f;
        } else if (BreakMajorIsImminent) {
            increment = 0.9f;
        } else if (BreakMinorIsImminent) {
            increment = 0.5f;
        }

        pawnTracker.bondChance += increment;
        pawnTracker.doCheckWhenThisIsZero = (pawnTracker.doCheckWhenThisIsZero + 1) % 100;
    }
}
