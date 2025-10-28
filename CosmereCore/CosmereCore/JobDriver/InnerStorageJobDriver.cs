using System;
using Cosmere.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.JobDriver;

public abstract class InnerStorageJobDriver : JobDriver_HaulToContainer {
    protected InnerStorage? storage => Container.TryGetComp<InnerStorage>();

    protected abstract IEnumerable<Toil> GetToils();

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, errorOnFailed: errorOnFailed)) {
            return false;
        }

        UpdateEnrouteTrackers();
        pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
        return true;
    }

    private void UpdateEnrouteTrackers() {
        int count = job.count;
        TryReserveEnroute(TargetThingC, ref count);
        if (TargetB != TargetC) {
            TryReserveEnroute(TargetThingB, ref count);
        }

        if (job.targetQueueB == null) return;

        foreach (LocalTargetInfo localTargetInfo in job.targetQueueB) {
            if (!TargetC.HasThing || !(localTargetInfo == (LocalTargetInfo)TargetThingC)) {
                TryReserveEnroute(localTargetInfo.Thing, ref count);
            }
        }
    }

    private void TryReserveEnroute(Verse.Thing thing, ref int count) {
        if (thing.DestroyedOrNull()) return;

        UpdateTracker(thing.TryGetComp<InnerStorage>(), ref count);
    }

    private void UpdateTracker(InnerStorage container, ref int count) {
        if (ThingToCarry.DestroyedOrNull() || container.Map == null) {
            return;
        }

        if (job.playerForced && container.GetSpaceRemainingWithEnroute(ThingDef) == 0) {
            container.Map.enrouteManager.InterruptEnroutePawns(container, pawn);
        }

        int count1 = Mathf.Min(count, container.GetSpaceRemainingWithEnroute(ThingDef));
        if (count1 > 0) {
            container.Map.enrouteManager.AddEnroute(
                container,
                pawn,
                TargetThingA.def,
                count1
            );
        }

        count -= count1;
    }

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