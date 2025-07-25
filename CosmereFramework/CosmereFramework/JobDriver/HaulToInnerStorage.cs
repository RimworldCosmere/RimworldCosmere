using Cosmere.Framework.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Framework.JobDriver;

public class HaulToInnerStorage : JobDriver_HaulToContainer {
    private InnerStorage? storage => Container.TryGetComp<InnerStorage>();

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOn(() => storage is null);
        this.FailOn(
            delegate {
                Verse.Thing thing = GetActor().jobs.curJob.GetTarget(TargetIndex.B).Thing;
                if (thing == null) {
                    return true;
                }

                if (storage!.parent.Destroyed) {
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

                if (!storage.innerContainer.CanAcceptAnyOf(ThingToCarry)) {
                    return true;
                }

                return !storage.Accepts(ThingToCarry);
            }
        );
        this.FailOnForbidden(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, true)
            .FailOn(() => ThingToCarry.ParentHolder is MinifiedThing)
            .FailOnSelfAndParentsDespawnedOrNull(TargetIndex.A);
        yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false, true, true);
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
    }
}