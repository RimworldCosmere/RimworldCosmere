using System.Collections.Generic;
using CosmereScadrial.Feruchemy.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.JobDriver;

public class EquipMetalmind : Verse.AI.JobDriver {
    private Verse.Thing metalmind => job.targetA.Thing;
    private Metalmind? metalmindComp => metalmind.TryGetComp<Metalmind>();

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!pawn.Reserve(metalmind, job, 1, 1, errorOnFailed: errorOnFailed)) {
            return false;
        }

        if (metalmindComp?.equipped != false) {
            return false;
        }

        job.count = 1;

        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
        
        yield return Toils_Haul.TakeToInventory(TargetIndex.A, job.count);
        yield return Toils_General.Wait(25).WithProgressBarToilDelay(TargetIndex.A);
        yield return Toils_General.Do(() => {
            metalmindComp!.equipped = true;
        });
    }
}