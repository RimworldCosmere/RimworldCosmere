using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using Verse;

namespace Cosmere.System.Roshar.LetterArrive;

public class BondsmithCallingLetter : ChoiceLetter {
    private string sprenName = "";

    public override bool ShouldAutomaticallyOpenLetter => true;

    public override IEnumerable<DiaOption> Choices {
        get {
            yield return new DiaOption("CRO_Bondsmith_Accept".Translate()) {
                action = () => {
                    Pawn pawn = lookTargets.PrimaryTarget.Pawn;
                    Find.WindowStack.Add(new ChooseRadiantOrder(pawn, sprenName));
                    Find.LetterStack.RemoveLetter(this);
                },
                resolveTree = true,
            };

            yield return new DiaOption("CRO_Bondsmith_NotYet".Translate()) {
                action = () => { Find.LetterStack.RemoveLetter(this); },
                resolveTree = true,
            };

            yield return new DiaOption("CRO_Bondsmith_Refuse".Translate()) {
                action = () => {
                    Pawn pawn = lookTargets.PrimaryTarget.Pawn;
                    BondsmithCalling? hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(
                        DefDatabase<HediffDef>.GetNamedSilentFail($"Cosmere_Roshar_Hediff_BondsmithCalling_{sprenName}")
                    ) as BondsmithCalling;
                    if (hediff != null) {
                        pawn.health!.RemoveHediff(hediff);
                    }

                    BondsmithCallingChecker? checker =
                        Current.Game?.GetComponent<BondsmithCallingChecker>();
                    checker?.RecordRefusal(pawn);

                    Find.LetterStack.RemoveLetter(this);
                },
                resolveTree = true,
            };

            if (lookTargets.IsValid()) {
                yield return Option_JumpToLocationAndPostpone;
            }
        }
    }

    public void Setup(string spren) {
        sprenName = spren;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref sprenName, "sprenName", "");
    }
}