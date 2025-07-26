using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Framework.JobDriver;

public class StoreInApparelInnerStorage : InnerStorageJobDriver {
    private Pawn? apparelWearer => storage?.ParentPawn;

    protected override IEnumerable<Toil> GetToils() {
        this.FailOn(() => apparelWearer != GetActor());

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, true)
            .FailOn(() => ThingToCarry.ParentHolder is MinifiedThing)
            .FailOnSelfAndParentsDespawnedOrNull(TargetIndex.A);
        yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false, true, true);

        yield return Toils_General.Open(TargetIndex.B);
        Toil toil = Toils_General.Wait(Duration);
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