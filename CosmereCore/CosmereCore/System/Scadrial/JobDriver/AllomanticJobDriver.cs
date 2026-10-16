using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Gene;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public abstract class AllomanticJobDriver : Verse.AI.JobDriver {
    private ILoadReferenceable? cachedSource;

    protected virtual Pawn? TargetPawn => TargetA.Pawn;

    protected virtual AllomancyAbility Ability => (AllomancyAbility)(job?.source ?? cachedSource)!;

    protected virtual Allomancer Gene => Ability.Gene;

    protected virtual bool TargetIsPawn => TargetPawn != null;

    protected override IEnumerable<Toil> MakeNewToils() {
        AddFinishAction(UpdateStatusToOff);

        yield break;
    }

    protected virtual void UpdateStatusToOff(JobCondition condition) {
        if (condition == JobCondition.Ongoing) return;
        if (Ability.status != BurningStatus.Off) {
            Ability.UpdateStatus(BurningStatus.Off);
        }
    }

    protected virtual void UpdateBurnRate(float desiredBurnRate) {
        Gene.UpdateDrainSource((Ability.Def, desiredBurnRate));
    }

    protected virtual bool ShouldStopJob() {
        return pawn.Downed || pawn.Dead || !pawn.CanUseMetal(Gene.Metal);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_References.Look(ref cachedSource, "cachedSource");

        if (Scribe.mode == LoadSaveMode.PostLoadInit && cachedSource != null) {
            job.source = cachedSource;
        }
    }
}
