using System.Collections.Generic;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using Verse;
using Verse.AI;

namespace CosmereScadrial.JobDriver;

public abstract class AllomanticJobDriver : Verse.AI.JobDriver {
    private ILoadReferenceable cachedSource;
    protected virtual Pawn targetPawn => TargetA.Pawn;
    protected virtual AbstractAbility ability => (AbstractAbility)(job?.source ?? cachedSource);
    protected virtual Allomancer gene => ability.gene;
    protected virtual bool targetIsPawn => targetPawn != null;


    protected override IEnumerable<Toil> MakeNewToils() {
        AddFinishAction(UpdateStatusToOff);

        yield break;
    }

    protected virtual void UpdateStatusToOff(JobCondition condition) {
        if (condition == JobCondition.Ongoing) return;
        if (ability.status != BurningStatus.Off) {
            ability.UpdateStatus(BurningStatus.Off);
        }
    }

    protected virtual void UpdateBurnRate(float desiredBurnRate) {
        gene.UpdateBurnSource((ability.def, desiredBurnRate));
    }

    protected virtual bool ShouldStopJob() {
        return pawn.Downed || pawn.Dead || !pawn.CanUseMetal(gene.metal);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_References.Look(ref cachedSource, "cachedSource");

        if (Scribe.mode == LoadSaveMode.PostLoadInit && cachedSource != null) {
            job.source = cachedSource;
        }
    }
}