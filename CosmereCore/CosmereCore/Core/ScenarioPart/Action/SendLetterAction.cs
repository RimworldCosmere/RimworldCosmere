using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

public class SendLetterAction : ProgressionAction {
    public string? letterDef;
    public string text = string.Empty;
    public string title = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        LetterDef def = LetterDefOf.NeutralEvent;
        if (letterDef != null) {
            LetterDef? resolved = DefDatabase<LetterDef>.GetNamedSilentFail(letterDef);
            if (resolved != null) def = resolved;
        }

        Find.LetterStack.ReceiveLetter(title, text, def);
    }
}
