using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public class UnequipMetalmind : Verse.AI.JobDriver {
    private Verse.Thing Metalmind => job.targetA.Thing;

    private Metalmind? MetalmindComp => Metalmind.TryGetComp<Metalmind>();

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!pawn.Reserve(Metalmind, job, 1, 1, errorOnFailed: errorOnFailed)) {
            return false;
        }

        if (Metalmind?.holdingOwner?.Owner != pawn) {
            return false;
        }

        if (MetalmindComp == null || !MetalmindComp.Equipped) {
            return false;
        }

        job.count = 1;

        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.B);

        yield return Toils_General.Wait(25).WithProgressBarToilDelay(TargetIndex.A);
        yield return Toils_General.Do(() => { MetalmindComp!.Equipped = false; });
    }
}
