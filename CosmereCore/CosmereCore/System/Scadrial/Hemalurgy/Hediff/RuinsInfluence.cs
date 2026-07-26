using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.Hediff;

public class RuinsInfluence : HediffWithComps {
    private int spikeCount;

    public void UpdateSpikeCount(int count) {
        spikeCount = count;
        Severity = count * 0.2f;
    }

    public override void Tick() {
        base.Tick();
        if (!pawn.IsHashIntervalTick(2500)) return;
        if (spikeCount < 4) return;

        float mentalBreakChance = (spikeCount - 3) * 0.02f;
        if (Rand.Chance(mentalBreakChance)) {
            pawn.mindState.mentalStateHandler.TryStartMentalState(
                MentalStateDefOf.Wander_Psychotic,
                "CS_Hemalurgy_RuinsControl".Translate(),
                forceWake: false,
                causedByMood: false
            );
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref spikeCount, "spikeCount");
    }
}