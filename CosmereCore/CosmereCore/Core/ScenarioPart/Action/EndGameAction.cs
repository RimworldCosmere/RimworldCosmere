using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Ends the campaign. For the branch of a story beat the colony does not come back from.
///     <para>
///         Two shapes, because vanilla has two. Credits treat it as a win and can drop the
///         player back to the main menu; the dialog is quieter and can leave them free to keep
///         playing a colony whose story is over.
///     </para>
/// </summary>
public class EndGameAction : ProgressionAction {
    /// <summary>Uses the plain dialog instead of the credits, with a keep-playing button.</summary>
    public bool allowKeepPlaying;

    public bool exitToMainMenu = true;
    public SongDef? song;
    public string textKey = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        string text = textKey.Length > 0 ? textKey.Translate().Resolve() : string.Empty;

        if (allowKeepPlaying) {
            GenGameEnd.EndGameDialogMessage(text, true);
            return;
        }

        GameVictoryUtility.ShowCredits(text, song, exitToMainMenu);
    }
}
