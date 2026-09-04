using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

public class RemoveGameConditionAction : ProgressionAction {
    public string gameCondition = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        GameConditionDef? def = DefDatabase<GameConditionDef>.GetNamedSilentFail(gameCondition);
        if (def == null) {
            Log.Warn($"ScenarioProgression: GameCondition '{gameCondition}' not found");
            return;
        }

        Map? map = Find.CurrentMap;
        if (map == null) return;

        GameCondition? active = map.gameConditionManager.GetActiveCondition(def);
        if (active != null) {
            active.End();
        }
    }
}
