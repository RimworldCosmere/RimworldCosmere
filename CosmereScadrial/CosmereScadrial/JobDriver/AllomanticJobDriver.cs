using System.Collections.Generic;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Extension;
using Verse;
using Verse.AI;

namespace CosmereScadrial.JobDriver;

public abstract class AllomanticJobDriver : Verse.AI.JobDriver {
    protected virtual Pawn targetPawn => TargetA.Pawn;
    protected virtual AbstractAbility ability => (AbstractAbility)job.source;
    protected virtual MetalBurning metalBurning => pawn.GetComp<MetalBurning>();
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
        metalBurning.UpdateBurnSource(ability.metal, desiredBurnRate, ability.def);
    }

    protected virtual bool ShouldStopJob() {
        return pawn.Downed || pawn.Dead || !pawn.CanUseMetal(ability.metal);
    }
}