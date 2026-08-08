using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Changes what a pawn is called, and tells the colony why.
/// </summary>
/// <remarks>
///     Written for the moment a kandra stops using the name of the person it replaced. The
///     colony has spent the whole campaign talking to OreSeur; finding out they were talking to
///     TenSoon is the story beat, and the name in the colonist bar changing is how it lands.
/// </remarks>
public class RenamePawnAction : ProgressionAction {
    public string pawnName = string.Empty;

    public string? newFirstName;
    public string? newLastName;
    public string? newNickName;

    public string? letterTitle;
    public string? letterText;

    /// <summary>
    ///     Set when the pawn simply not being there is a normal outcome. A kandra who never
    ///     joined, or died on the way, should not log a fault every campaign.
    /// </summary>
    public bool optional = true;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            if (optional) {
                Logger.Verbose($"ScenarioProgression: '{pawnName}' is not here, so there is nobody to rename.");
            } else {
                Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for RenamePawn");
            }

            return;
        }

        if (string.IsNullOrEmpty(newFirstName)) {
            Logger.Warning("ScenarioProgression: RenamePawn has no newFirstName, so nothing changed.");
            return;
        }

        pawn.Name = Rename(pawn, newFirstName!, newLastName, newNickName);

        if (letterTitle != null && letterText != null) {
            Find.LetterStack.ReceiveLetter(letterTitle, letterText, LetterDefOf.NeutralEvent, pawn);
        }

        Logger.Info($"ScenarioProgression: '{pawnName}' is now called {pawn.Name.ToStringShort}.");
    }

    /// <summary>
    ///     Keeps a triple name a triple name and a single a single, so the colonist bar and the
    ///     social tab do not start disagreeing about what to call somebody.
    /// </summary>
    private static Name Rename(Pawn pawn, string first, string? last, string? nick) {
        if (pawn.Name is NameTriple existing) {
            return new NameTriple(
                first,
                nick ?? existing.Nick,
                last ?? existing.Last
            );
        }

        if (!string.IsNullOrEmpty(last) || !string.IsNullOrEmpty(nick)) {
            return new NameTriple(first, nick ?? first, last ?? string.Empty);
        }

        return new NameSingle(first);
    }
}
