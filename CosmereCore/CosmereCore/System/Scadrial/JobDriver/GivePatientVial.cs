using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public class GivePatientVial : Verse.AI.JobDriver {
    private const float FeedDurationMultiplier = 1.5f;

    private Verse.Thing Vial => job.targetA.Thing;

    private Pawn Deliveree => job.targetB.Pawn;

    private Pawn_InventoryTracker? VialHolderInventory => Vial?.ParentHolder as Pawn_InventoryTracker;

    private Pawn VialHolder => job.targetC.Pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!pawn.Reserve(Deliveree, job, errorOnFailed: errorOnFailed)) {
            return false;
        }

        if (pawn.inventory != null && pawn.inventory.Contains(TargetThingA)) {
            return true;
        }

        if (!pawn.Reserve(Vial, job, 10, 1, errorOnFailed: errorOnFailed)) {
            return false;
        }

        job.count = 1;

        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
        Toil carryVialFromInventory = Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
        Toil goToVialHolder = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch)
            .FailOn(() => VialHolder != VialHolderInventory?.pawn || VialHolder.IsForbidden(pawn));
        Toil carryVialToPatient = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);

        // ISSUE: reference to a compiler-generated method
        yield return Toils_Jump.JumpIf(
            carryVialFromInventory,
            () => pawn.inventory != null && pawn.inventory.Contains(TargetThingA)
        );
        yield return Toils_Haul.CheckItemCarriedByOtherPawn(Vial, TargetIndex.C, goToVialHolder);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
        yield return Toils_Ingest.PickupIngestible(TargetIndex.A, Deliveree);
        yield return Toils_Jump.Jump(carryVialToPatient);
        yield return goToVialHolder;
        yield return Toils_General.Wait(25).WithProgressBarToilDelay(TargetIndex.C);
        yield return Toils_Haul.TakeFromOtherInventory(
            Vial,
            pawn.inventory.innerContainer,
            VialHolderInventory?.innerContainer,
            job.count,
            TargetIndex.A
        );
        yield return carryVialFromInventory;
        yield return carryVialToPatient;
        yield return Toils_Ingest.ChewIngestible(Deliveree, FeedDurationMultiplier, TargetIndex.A)
            .FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
        yield return Toils_Ingest.FinalizeIngest(Deliveree, TargetIndex.A);
    }
}
