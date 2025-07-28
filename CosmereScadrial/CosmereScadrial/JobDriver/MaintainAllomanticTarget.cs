using Cosmere.Scadrial.Allomancy.Ability;
using Cosmere.Scadrial.Allomancy.Hediff;
using Cosmere.Scadrial.Comp.Hediff;
using Cosmere.Scadrial.Utility;
using Verse;
using Verse.AI;

namespace Cosmere.Scadrial.JobDriver;

public class MaintainAllomanticTarget : AllomanticJobDriver {
    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(
            targetPawn,
            job,
            errorOnFailed: errorOnFailed,
            maxPawns: int.MaxValue,
            stackCount: 1,
            ignoreOtherReservations: true
        );
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        foreach (Toil baseToil in base.MakeNewToils()) {
            yield return baseToil;
        }

        this.FailOnDespawnedNullOrForbidden(TargetIndex.A)
            .FailOnDowned(TargetIndex.A);

        Toil toil = new Toil {
            defaultCompleteMode = ToilCompleteMode.Never,
            tickAction = () => {
                if (ShouldStopJob()) {
                    ability.UpdateStatus(BurningStatus.Off);
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                // Maintain proximity
                pawn.MaintainProximityTo(
                    TargetA,
                    job.followRadius,
                    targetIsPawn ? PathEndMode.Touch : PathEndMode.OnCell
                );

                // If we aren't close enough to the pawn, update burnrate to 0, and get closer
                if (pawn.DistanceTo(targetPawn!) > ability.def.verbProperties.range) {
                    UpdateBurnRate(0f);
                    return;
                }

                SurgeChargeHediff? surge = AllomancyUtility.GetSurgeBurn(pawn);
                surge?.Burn();

                // Ensure the burn rate is set properly
                MaintainBurnRate();

                // Apply and maintain effect
                MaintainEffectOnTarget();

                if (surge == null) return;
                surge.PostBurn();
                EndJobWith(JobCondition.Succeeded);
            },
        };

        yield return toil;
    }

    protected override bool ShouldStopJob() {
        if (targetIsPawn && targetPawn != null && (targetPawn.Dead || targetPawn.Downed)) {
            return true;
        }

        return base.ShouldStopJob() ||
               !pawn.CanReach(TargetA, targetIsPawn ? PathEndMode.Touch : PathEndMode.OnCell, Danger.None);
    }

    protected virtual void MaintainBurnRate() {
        UpdateBurnRate(ability.GetDesiredBurnRateForStatus());
    }

    protected virtual void MaintainEffectOnTarget() {
        AllomanticHediff? hediff = (AllomanticHediff?)targetPawn!.GetOrAddHediff(pawn, ability, ability.def);

        hediff?.TryGetComp<DisappearsScaled>()?.CompPostMake();
    }
}