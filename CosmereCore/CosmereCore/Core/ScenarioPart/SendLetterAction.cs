using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class SendLetterAction : ProgressionAction {
    public string title = "";
    public string text = "";
    public string? letterDef;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        LetterDef def = LetterDefOf.NeutralEvent;
        if (letterDef != null) {
            LetterDef? resolved = DefDatabase<LetterDef>.GetNamedSilentFail(letterDef);
            if (resolved != null) def = resolved;
        }

        Find.LetterStack.ReceiveLetter(title, text, def);
    }
}
