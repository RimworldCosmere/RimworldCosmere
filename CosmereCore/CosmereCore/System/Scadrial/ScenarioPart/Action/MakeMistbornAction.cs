using Cosmere.Core.ScenarioPart;
using Cosmere.Core.ScenarioPart.Action;
using Verse;

using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.ScenarioPart.Action;

/// <summary>
///     Burns a bead of lerasium into someone. Snapped on the spot rather than left dormant -
///     this is the moment the metal takes, not a connection waiting to be found.
/// </summary>
public class MakeMistbornAction : ProgressionAction {
    public string? cause;
    public string pawnName = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for MakeMistborn");
            return;
        }

        GeneUtility.AddMistborn(pawn, false, true, cause);
    }

    public override string? Describe() {
        return "CS_Progression_Effect_Mistborn".Translate(pawnName.Named("PAWN")).Resolve();
    }
}
