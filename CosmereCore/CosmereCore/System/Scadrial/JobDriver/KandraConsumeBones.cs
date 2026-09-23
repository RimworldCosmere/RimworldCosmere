using Cosmere.System.Scadrial.Kandra;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

/// <summary>
///     A kandra taking a corpse's bones into itself and learning the body.
/// </summary>
/// <remarks>
///     The corpse is destroyed. There is nothing left to bury, which is the part that makes
///     colonies nervous about keeping one around.
/// </remarks>
public class KandraConsumeBones : Verse.AI.JobDriver {
    private const int WorkDuration = 2500;

    private Corpse Corpse => (Corpse)job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        AddFailCondition(() => pawn.TryGetComp<CompKandraForms>() == null);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
            .FailOnForbidden(TargetIndex.A);
        yield return Toils_General.Wait(WorkDuration, TargetIndex.A)
            .WithProgressBarToilDelay(TargetIndex.A)
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch);

        Toil consume = ToilMaker.MakeToil("KandraConsumeBones");
        consume.initAction = Consume;
        consume.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return consume;
    }

    private void Consume() {
        CompKandraForms? forms = pawn.TryGetComp<CompKandraForms>();
        if (forms == null) return;

        Pawn eaten = Corpse.InnerPawn;
        forms.Learn(eaten);

        Messages.Message(
            "CS_Kandra_LearnedForm".Translate(
                pawn.LabelShortCap.Named("PAWN"),
                eaten.LabelShortCap.Named("FORM")
            ),
            pawn,
            MessageTypeDefOf.NeutralEvent,
            false
        );

        Corpse.Destroy();
    }
}
