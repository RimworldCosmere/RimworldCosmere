using Cosmere.Core.Def;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

public class TriggerIncidentAction : ProgressionAction {
    public string incident = string.Empty;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        IncidentDef? def = DefDatabase<IncidentDef>.GetNamedSilentFail(incident);
        if (def == null) {
            Log.Warn($"ScenarioProgression: Incident '{incident}' not found");
            return;
        }

        if (!WorldMatches(incident)) {
            Log.Warn(
                $"ScenarioProgression: Incident '{incident}' belongs to another world than " +
                $"'{WorldUtility.Primary?.defName}'; skipping it"
            );
            return;
        }

        Map? map = Find.CurrentMap;
        if (map == null) return;

        IncidentParms parms = StorytellerUtility.DefaultParmsNow(def.category, map);
        def.Worker.TryExecute(parms);
    }

    private static bool WorldMatches(string incidentDefName) {
        CosmereWorldDef? owner = WorldUtility.WorldForDefName(incidentDefName);
        CosmereWorldDef? primary = WorldUtility.Primary;

        return WorldGate.Matches(owner?.defName, primary?.defName, primary?.crossWorld ?? false);
    }
}
