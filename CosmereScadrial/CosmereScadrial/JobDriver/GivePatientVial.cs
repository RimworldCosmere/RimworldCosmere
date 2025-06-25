using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.JobDriver;

public class GivePatientVial : Verse.AI.JobDriver {
    private const float FeedDurationMultiplier = 1.5f;

    private Verse.Thing vial => job.targetA.Thing;
    private Pawn deliveree => job.targetB.Pawn;
    private Pawn_InventoryTracker? vialHolderInventory => vial?.ParentHolder as Pawn_InventoryTracker;
    private Pawn vialHolder => job.targetC.Pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!pawn.Reserve(deliveree, job, errorOnFailed: errorOnFailed)) {
            return false;
        }

        if (pawn.inventory != null && pawn.inventory.Contains(TargetThingA)) {
            return true;
        }

        if (!pawn.Reserve(vial, job, 10, 1, errorOnFailed: errorOnFailed)) {
            return false;
        }

        job.count = 1;

        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
        Toil carryVialFromInventory = Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
        Toil goToVialHolder = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch)
            .FailOn(() => vialHolder != vialHolderInventory?.pawn || vialHolder.IsForbidden(pawn));
        Toil carryVialToPatient = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
        // ISSUE: reference to a compiler-generated method
        yield return Toils_Jump.JumpIf(carryVialFromInventory,
            () => pawn.inventory != null && pawn.inventory.Contains(TargetThingA));
        yield return Toils_Haul.CheckItemCarriedByOtherPawn(vial, TargetIndex.C, goToVialHolder);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
        yield return Toils_Ingest.PickupIngestible(TargetIndex.A, deliveree);
        yield return Toils_Jump.Jump(carryVialToPatient);
        yield return goToVialHolder;
        yield return Toils_General.Wait(25).WithProgressBarToilDelay(TargetIndex.C);
        yield return Toils_Haul.TakeFromOtherInventory(vial, pawn.inventory.innerContainer,
            vialHolderInventory?.innerContainer, job.count, TargetIndex.A);
        yield return carryVialFromInventory;
        yield return carryVialToPatient;
        yield return Toils_Ingest.ChewIngestible(deliveree, FeedDurationMultiplier, TargetIndex.A)
            .FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
        yield return Toils_Ingest.FinalizeIngest(deliveree, TargetIndex.A);
        ;
    }
}