using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Comp.Hediff;
using Cosmere.System.Scadrial.Util;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public class MaintainAllomanticTarget : AllomanticJobDriver {
    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(
            TargetPawn,
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
                    Ability.UpdateStatus(BurningStatus.Off);
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                // Maintain proximity
                pawn.MaintainProximityTo(
                    TargetA,
                    job.followRadius,
                    TargetIsPawn ? PathEndMode.Touch : PathEndMode.OnCell
                );

                // If we aren't close enough to the pawn, update burnrate to 0, and get closer
                if (pawn.DistanceTo(TargetPawn!) > Ability.Def.verbProperties.range) {
                    UpdateBurnRate(0f);
                    return;
                }

                SurgeChargeHediff? surge = AllomancyUtility.FindSurgeChargeHediff(pawn);
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
        if (TargetIsPawn && TargetPawn != null && (TargetPawn.Dead || TargetPawn.Downed)) {
            return true;
        }

        return base.ShouldStopJob() ||
               !pawn.CanReach(TargetA, TargetIsPawn ? PathEndMode.Touch : PathEndMode.OnCell, Danger.None);
    }

    protected virtual void MaintainBurnRate() {
        UpdateBurnRate(Ability.GetDesiredBurnRateForStatus());
    }

    protected virtual void MaintainEffectOnTarget() {
        AllomanticHediff? hediff = TargetPawn!.GetOrAddHediff(Ability, Ability.Def, pawn);

        hediff?.TryGetComp<DisappearsScaled>()?.CompPostMake();
    }
}
