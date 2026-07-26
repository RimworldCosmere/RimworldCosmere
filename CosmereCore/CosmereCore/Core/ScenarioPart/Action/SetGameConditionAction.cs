
using RimWorld;
using Verse;
namespace Cosmere.Core.ScenarioPart.Action;

public class SetGameConditionAction : ProgressionAction {
    public int durationDays = 1;
    public string gameCondition = "";

    public override void Execute(GameComponent_ScenarioProgression comp) {
        GameConditionDef? def = DefDatabase<GameConditionDef>.GetNamedSilentFail(gameCondition);
        if (def == null) {
            Logger.Warning($"ScenarioProgression: GameCondition '{gameCondition}' not found");
            return;
        }

        Map? map = Find.CurrentMap;
        if (map == null) return;

        GameCondition condition = GameConditionMaker.MakeCondition(def, durationDays * GenDate.TicksPerDay);
        map.gameConditionManager.RegisterCondition(condition);
    }
}