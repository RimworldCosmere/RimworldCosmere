using Cosmere.Framework.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Framework.JobDriver;

public class HaulToApparel : JobDriver_HaulToContainer {
    private ApparelWithStorage storage => (ApparelWithStorage)Container;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return base.TryMakePreToilReservations(errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOn(() => Container is not ApparelWithStorage);
        this.FailOn(
            delegate {
                Verse.Thing thing = GetActor().jobs.curJob.GetTarget(TargetIndex.B).Thing;
                if (thing == null) {
                    return true;
                }

                if (storage.Destroyed) {
                    if (job.targetQueueB.NullOrEmpty()) {
                        return true;
                    }

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
                }

                ThingOwner thingOwner = storage.TryGetInnerInteractableThingOwner();
                if (thingOwner != null && !thingOwner.CanAcceptAnyOf(ThingToCarry)) {
                    return true;
                }

                if (!storage.Accepts(ThingToCarry)) return true;

                return false;
            }
        );
        this.FailOnForbidden(TargetIndex.B);
        Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, true)
            .FailOn(() => ThingToCarry.ParentHolder is MinifiedThing)
            .FailOnSelfAndParentsDespawnedOrNull(TargetIndex.A);
        Toil uninstallIfMinifiable = Toils_Construct.UninstallIfMinifiable(TargetIndex.A)
            .FailOnSomeonePhysicallyInteracting(TargetIndex.A)
            .FailOn(() => ThingToCarry.ParentHolder is MinifiedThing)
            .FailOnSelfAndParentsDespawnedOrNull(TargetIndex.A)
            .FailOnDestroyedOrNull(TargetIndex.A);
        Toil startCarryingThing = Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false, true, true);
        Toil jumpIfAlsoCollectingNextTarget =
            Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(getToHaulTarget, TargetIndex.A);
        Toil carryToContainer = Toils_Haul.CarryHauledThingToContainer();
        yield return Toils_Jump.JumpIf(jumpIfAlsoCollectingNextTarget, () => pawn.IsCarryingThing(ThingToCarry));
        yield return getToHaulTarget;
        yield return uninstallIfMinifiable;
        yield return startCarryingThing;
        yield return jumpIfAlsoCollectingNextTarget;
        yield return carryToContainer;
        Toil toil = Toils_General.Wait(Duration, TargetIndex.B);
        toil.WithProgressBarToilDelay(TargetIndex.B);
        EffecterDef workEffecter = WorkEffecter;
        if (workEffecter != null) {
            toil.WithEffect(workEffecter, TargetIndex.B);
        }

        SoundDef workSustainer = WorkSustainer;
        if (workSustainer != null) {
            toil.PlaySustainerOrSound(workSustainer);
        }

        ModifyPrepareToil(toil);
        yield return toil;
        yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.None);
        yield return Toils_Haul.JumpToCarryToNextContainerIfPossible(carryToContainer, TargetIndex.None);
    }
}