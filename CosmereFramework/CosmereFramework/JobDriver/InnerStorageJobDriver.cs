using System;
using Cosmere.Framework.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Framework.JobDriver;

public abstract class InnerStorageJobDriver : JobDriver_HaulToContainer {
    protected InnerStorage? storage => Container.TryGetComp<InnerStorage>();

    protected abstract IEnumerable<Toil> GetToils();

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOn(() => storage is null);
        this.FailOn(() => {
                Verse.Thing thing = GetActor().jobs.curJob.GetTarget(TargetIndex.B).Thing;
                if (thing == null || storage == null) {
                    return true;
                }

                if (!storage.parent.Destroyed) return !storage.Accepts(ThingToCarry);

                if (job.targetQueueB.NullOrEmpty()) return true;

                if (!Toils_Haul.TryGetNextDestinationFromQueue(
                        TargetIndex.C,
                        TargetIndex.B,
                        ThingDef,
                        job,
                        pawn,
                        out Verse.Thing? nextTarget
                    )) {
                    return true;
                }

                job.targetQueueB.RemoveAll(target => target.Thing == nextTarget);
                job.targetB = nextTarget;

                return !storage.Accepts(ThingToCarry);
            }
        );
        this.FailOnForbidden(TargetIndex.B);

        foreach (Toil toil in GetToils()) {
            yield return toil;
        }
    }

    protected Toil CarryHauledThingToContainer() {
        Toil gotoDest = ToilMaker.MakeToil();
        gotoDest.initAction = () => gotoDest.actor.pather.StartPath(
            Container,
            PathEndMode.Touch
        );

        gotoDest.AddFailCondition(
            (Func<bool>)(() => {
                if (storage == null) return true;
                if (Container.Destroyed ||
                    !gotoDest.actor.jobs.curJob.ignoreForbidden && Container.IsForbidden(gotoDest.actor)) {
                    return true;
                }

                ThingOwner? interactableThingOwner = storage.innerContainer;
                return interactableThingOwner != null &&
                       !interactableThingOwner.CanAcceptAnyOf(gotoDest.actor.carryTracker.CarriedThing);
            })
        );
        gotoDest.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        return gotoDest;
    }
}