using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

public class SendLetterAction : ProgressionAction {
    public string? letterDef;

    /// <summary>Set false on a card whose block changes nothing worth spelling out.</summary>
    public bool listEffects = true;

    public string text = string.Empty;
    public string title = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        LetterDef def = LetterDefOf.NeutralEvent;
        if (letterDef != null) {
            LetterDef? resolved = DefDatabase<LetterDef>.GetNamedSilentFail(letterDef);
            if (resolved != null) def = resolved;
        }

        string body = text;
        string? effects = comp.PendingEffects;
        if (listEffects && effects != null && effects.Length > 0) {
            body += "\n\n" + "CC_Progression_Effects_Header".Translate() + "\n" + effects;
        }

        Find.LetterStack.ReceiveLetter(title, body, def);
    }
}
