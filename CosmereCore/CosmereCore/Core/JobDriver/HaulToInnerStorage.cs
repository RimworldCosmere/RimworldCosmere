using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Core.JobDriver;

public class HaulToInnerStorage : InnerStorageJobDriver {
    protected override IEnumerable<Toil> GetToils() {
        Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, true)
            .FailOn(() => ThingToCarry.ParentHolder is MinifiedThing)
            .FailOnSelfAndParentsDespawnedOrNull(TargetIndex.A);
        Toil jumpIfAlsoCollectingNextTarget =
            Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(getToHaulTarget, TargetIndex.A);
        Toil startCarryingThing = Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false, true, true);
        Toil carryToContainer = CarryHauledThingToContainer();

        yield return Toils_Jump.JumpIf(jumpIfAlsoCollectingNextTarget, () => pawn.IsCarryingThing(ThingToCarry));
        yield return getToHaulTarget;
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
        yield return Toils_Haul.JumpToCarryToNextContainerIfPossible(carryToContainer, TargetIndex.C);
    }
}
