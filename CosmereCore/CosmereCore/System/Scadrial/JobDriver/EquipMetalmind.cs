using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public class EquipMetalmind : Verse.AI.JobDriver {
    private Verse.Thing Metalmind => job.targetA.Thing;

    private Metalmind? MetalmindComp => Metalmind.TryGetComp<Metalmind>();

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!pawn.Reserve(Metalmind, job, 1, 1, errorOnFailed: errorOnFailed)) {
            return false;
        }

        if (MetalmindComp == null || MetalmindComp.Equipped) {
            return false;
        }

        job.count = 1;

        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOn(() => !GetActor().health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
        this.FailOn(() => MassUtility.CountToPickUpUntilOverEncumbered(pawn, job.targetA.Thing) == 0);
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
            .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
        yield return Toils_General.Wait(50).WithProgressBarToilDelay(TargetIndex.A);
        yield return Toils_Haul.TakeToInventory(TargetIndex.A, job.count);
        yield return Toils_General.Do(() => { MetalmindComp!.Equipped = true; });
    }
}
