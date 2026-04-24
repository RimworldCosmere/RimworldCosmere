using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class TriggerIncidentAction : ProgressionAction {
    public string incident = "";

    public override void Execute(GameComponent_ScenarioProgression comp) {
        IncidentDef? def = DefDatabase<IncidentDef>.GetNamedSilentFail(incident);
        if (def == null) {
            Logger.Warning($"ScenarioProgression: Incident '{incident}' not found");
            return;
        }

        Map? map = Find.CurrentMap;
        if (map == null) return;

        IncidentParms parms = StorytellerUtility.DefaultParmsNow(def.category, map);
        def.Worker.TryExecute(parms);
    }
}