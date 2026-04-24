using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.LetterArrive;

public class SpeakOath : ChoiceLetter {
    private int idealLevel;
    private Pawn? targetPawn;

    public override bool ShouldAutomaticallyOpenLetter => true;

    public override IEnumerable<DiaOption> Choices {
        get {
            if (targetPawn == null) {
                yield return Option_Close;
                yield break;
            }

            Surgebinder? surgebinder = targetPawn.genes?.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder == null) {
                yield return Option_Close;
                yield break;
            }

            DiaOption openDialog = new DiaOption("CRO_SpeakOath_ViewOath".Translate()) {
                action = () => {
                    Find.WindowStack.Add(
                        new RadiantOrderInfoDialog(
                            targetPawn,
                            surgebinder,
                            RadiantOrderInfoMode.SpeakOath
                        )
                    );
                    Find.LetterStack.RemoveLetter(this);
                },
                resolveTree = true,
            };
            yield return openDialog;
            yield return Option_Close;
        }
    }

    public void Setup(Pawn pawn, int nextIdeal) {
        targetPawn = pawn;
        idealLevel = nextIdeal;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref targetPawn!, "targetPawn");
        Scribe_Values.Look(ref idealLevel, "idealLevel");
    }
}