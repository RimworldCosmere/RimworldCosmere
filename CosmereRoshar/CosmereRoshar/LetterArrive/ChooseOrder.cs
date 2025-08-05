using Cosmere.Roshar.Dialog;
using Verse;

namespace Cosmere.Roshar.LetterArrive;

public class ChooseOrder : ChoiceLetter {
    public override bool ShouldAutomaticallyOpenLetter => true;

    public override IEnumerable<DiaOption> Choices {
        get {
            yield return OpenRadiantSelectionDialog();
            if (lookTargets.IsValid()) {
                yield return Option_JumpToLocation;
            }

            yield return Option_Close;
        }
    }

    private DiaOption OpenRadiantSelectionDialog() {
        DiaOption option = new DiaOption("CRO_Choose_Radiant_Order_Button".Translate()) {
            action = () => {
                Find.WindowStack.Add(new ChooseRadiantOrder(lookTargets.PrimaryTarget.Pawn));
                Find.LetterStack.RemoveLetter(this);
            },
            resolveTree = true,
        };

        return option;
    }
}