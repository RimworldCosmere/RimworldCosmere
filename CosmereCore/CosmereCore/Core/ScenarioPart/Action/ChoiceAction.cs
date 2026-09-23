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
        Execute(comp, null);
    }

    /// <summary>
    ///     Asks the fork, then runs <paramref name="continuation" /> once a branch has been
    ///     taken. The continuation is the rest of the event - era advances, handoffs - which must
    ///     not run while the question is still on screen.
    /// </summary>
    public void Execute(GameComponent_ScenarioProgression comp, global::System.Action? continuation) {
        string? required = requiresPawn;
        if (required != null && required.Length > 0 && comp.FindPawnByName(required) == null) {
            bool explicitPath = onMissing.Count > 0;
            Log.Info(
                $"ScenarioProgression: '{required}' is gone, taking the " +
                (explicitPath ? "onMissing" : "onAccept") + " path without asking."
            );
            Run(explicitPath ? onMissing : onAccept, comp);
            continuation?.Invoke();
            return;
        }

        Log.Info($"ScenarioProgression: asking '{titleKey}' - the campaign waits on it.");

        comp.AskChoice(() => new Dialog_ProgressionChoice(
            titleKey.Translate(),
            textKey.Translate(),
            new List<ProgressionChoiceOption> {
                new ProgressionChoiceOption(
                    acceptKey.Translate(),
                    acceptTipKey.Length > 0 ? acceptTipKey.Translate().Resolve() : null,
                    () => Resolve(onAccept, comp, continuation, acceptKey)
                ),
                new ProgressionChoiceOption(
                    declineKey.Translate(),
                    declineTipKey.Length > 0 ? declineTipKey.Translate().Resolve() : null,
                    () => Resolve(onDecline, comp, continuation, declineKey)
                ),
            }
        ));
    }

    private static void Resolve(
        List<ProgressionAction> branch,
        GameComponent_ScenarioProgression comp,
        global::System.Action? continuation,
        string chosenKey
    ) {
        Log.Info($"ScenarioProgression: '{chosenKey}' chosen.");
        comp.ChoiceAnswered();
        Run(branch, comp);
        continuation?.Invoke();
    }

    private static void Run(List<ProgressionAction> actions, GameComponent_ScenarioProgression comp) {
        comp.RunActions(actions);
    }
}
