using Cosmere.Core.ScenarioPart;
using Cosmere.Core.ScenarioPart.Action;
using Cosmere.System.Scadrial.Comp.Map;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.ScenarioPart.Action;

/// <summary>
///     Changes how hard the mists press on anyone caught out in them. The baseline is one in
///     sixteen per exposed hour; Ruin leaning on the world is what makes that number move.
/// </summary>
public class SetMistSnapOddsAction : ProgressionAction {
    /// <summary>One-in-N per exposed hour. Lower is more dangerous.</summary>
    public int oneIn = 16;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        MistsWatcher.SnapOneIn = oneIn < 1 ? 1 : oneIn;
        Logger.Important($"ScenarioProgression: the mists now take one in {MistsWatcher.SnapOneIn}.");
    }

    public override string? Describe() {
        return "CS_Progression_Effect_MistOdds".Translate(oneIn.Named("COUNT")).Resolve();
    }
}
