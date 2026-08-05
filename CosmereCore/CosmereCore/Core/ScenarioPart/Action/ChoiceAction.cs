using System.Collections.Generic;
using Cosmere.Core.UI;
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

    /// <summary>
    ///     What each branch actually does, shown on hover. A fork that ends the campaign has to
    ///     look different from one that does not before it is clicked, and a bare button cannot
    ///     say so.
    /// </summary>
    public string acceptTipKey = string.Empty;

    public string declineTipKey = string.Empty;

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

        Find.WindowStack.Add(
            new Dialog_ProgressionChoice(
                titleKey.Translate(),
                textKey.Translate(),
                new List<ProgressionChoiceOption> {
                    new ProgressionChoiceOption(
                        acceptKey.Translate(),
                        acceptTipKey.Length > 0 ? acceptTipKey.Translate().Resolve() : null,
                        () => Run(onAccept, comp)
                    ),
                    new ProgressionChoiceOption(
                        declineKey.Translate(),
                        declineTipKey.Length > 0 ? declineTipKey.Translate().Resolve() : null,
                        () => Run(onDecline, comp)
                    ),
                }
            )
        );
    }

    private static void Run(List<ProgressionAction> actions, GameComponent_ScenarioProgression comp) {
        comp.RunActions(actions);
    }
}
