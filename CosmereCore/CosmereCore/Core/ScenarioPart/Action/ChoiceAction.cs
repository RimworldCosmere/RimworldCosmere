using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Puts a narrative fork to the player and runs one branch or the other. Progression
///     events are otherwise fire-and-forget, so this is the only way a story beat can turn on
///     something the player decides rather than something the colony happens to be.
/// </summary>
public class ChoiceAction : ProgressionAction {
    public List<ProgressionAction> onAccept = [];
    public List<ProgressionAction> onDecline = [];

    /// <summary>
    ///     Runs instead of the dialog when requiresPawn names someone the colony no longer has.
    ///     Leave it empty and onAccept runs instead: a beat that hinges on one pawn usually
    ///     still happened, the player just did not get a say in it.
    /// </summary>
    public List<ProgressionAction> onMissing = [];

    /// <summary>Whose presence the fork depends on. Empty means always ask.</summary>
    public string? requiresPawn;

    public string acceptKey = string.Empty;
    public string declineKey = string.Empty;
    public string textKey = string.Empty;
    public string titleKey = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        string? required = requiresPawn;
        if (required != null && required.Length > 0 && comp.FindPawnByName(required) == null) {
            bool explicitPath = onMissing.Count > 0;
            Logger.Info(
                $"ScenarioProgression: '{required}' is gone, taking the " +
                (explicitPath ? "onMissing" : "onAccept") + " path."
            );
            Run(explicitPath ? onMissing : onAccept, comp);
            return;
        }

        // Two explicit buttons rather than CreateConfirmation: that helper only takes a confirm
        // callback, and this fork has to do something on both answers. forcePause and a null
        // cancel keep the player from dismissing it without choosing.
        Dialog_MessageBox dialog = new Dialog_MessageBox(
            textKey.Translate(),
            acceptKey.Translate(),
            () => Run(onAccept, comp),
            declineKey.Translate(),
            () => Run(onDecline, comp),
            titleKey.Translate(),
            false,
            null,
            null,
            WindowLayer.Dialog
        ) {
            forcePause = true,
            closeOnClickedOutside = false,
            closeOnCancel = false,
            closeOnAccept = false,
        };

        Find.WindowStack.Add(dialog);
    }

    private static void Run(List<ProgressionAction> actions, GameComponent_ScenarioProgression comp) {
        for (int i = 0; i < actions.Count; i++) {
            actions[i].Execute(comp);
        }
    }
}
