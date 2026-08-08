using Cosmere.System.Scadrial.Kandra;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

/// <summary>
///     Ten seconds of a kandra rearranging itself into somebody else, or back into itself.
/// </summary>
/// <remarks>
///     <c>job.count</c> carries the index into the kandra's remembered forms, or -1 to go back
///     to its own body. A job cannot hold an arbitrary object, and the index is stable for as
///     long as the job runs because nothing removes forms from the list.
/// </remarks>
public class KandraChangeShape : Verse.AI.JobDriver {
    public const int RevertIndex = -1;

    private CompKandraForms? Forms => pawn.TryGetComp<CompKandraForms>();

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        AddFailCondition(() => Forms == null);

        yield return Toils_General.Wait(KandraShapeshift.TicksToChange)
            .WithProgressBarToilDelay(TargetIndex.A);

        Toil change = ToilMaker.MakeToil("KandraChangeShape");
        change.initAction = Change;
        change.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return change;
    }

    private void Change() {
        CompKandraForms? forms = Forms;
        if (forms == null) return;

        if (job.count == RevertIndex) {
            KandraShapeshift.Revert(pawn);
            return;
        }

        if (job.count < 0 || job.count >= forms.Known.Count) return;

        KandraShapeshift.Wear(pawn, forms.Known[job.count]);
    }
}
