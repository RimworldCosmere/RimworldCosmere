using RimWorld;
using Verse;

namespace Cosmere.Core.Quest;

/// <summary>
///     Delivers Threat-kind quests. Separate from IncidentWorker_CosmereQuest because a Threat
///     is not an offer: it arrives already accepted, with no dec‍line and no acceptance deadline.
///     CosmereQuestEligibility keeps Threats out of the storyteller's offer pool, so this is
///     their only route to the player.
/// </summary>
public class IncidentWorker_CosmereThreat : IncidentWorker {
    protected override bool CanFireNowSub(IncidentParms parms) {
        if (!base.CanFireNowSub(parms)) return false;
        if (!(parms.target is Verse.Map map)) return false;

        CosmereQuestManager? manager = Current.Game?.GetComponent<CosmereQuestManager>();
        return manager != null && manager.PickThreat(map) != null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms) {
        if (!(parms.target is Verse.Map map)) return false;

        CosmereQuestManager? manager = Current.Game?.GetComponent<CosmereQuestManager>();
        if (manager == null) return false;

        CosmereQuestDef? questDef = manager.PickThreat(map);
        if (questDef == null) return false;

        return manager.TryStartThreat(questDef, map, null);
    }
}
