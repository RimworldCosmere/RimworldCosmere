using RimWorld;
using Verse;

namespace Cosmere.Core.Quest;

/// <summary>
///     The single IncidentDef that fires for every repeatable Cosmere quest. The storyteller
///     only knows about this one incident; CosmereQuestManager.PickWeighted decides which
///     CosmereQuestDef it actually offers, weighted and filtered by CosmereQuestEligibility.
///     Capstone quests are started separately through CosmereQuestManager.TryStartCapstone,
///     not through this incident.
/// </summary>
public class IncidentWorker_CosmereQuest : IncidentWorker {
    /// <summary>
    ///     Re-rolls PickWeighted rather than reusing whatever TryExecuteWorker later picks.
    ///     def.Worker is a single instance cached on the IncidentDef and reused for every
    ///     future firing attempt, so caching a picked def on this instance between the two
    ///     calls could be overwritten by an unrelated CanFireNowSub check before
    ///     TryExecuteWorker runs. The two calls are free to disagree on which specific quest
    ///     is eligible - PickWeighted's roll is random, but the underlying eligible set does
    ///     not change between them, and TryExecuteWorker validates its own pick independently
    ///     through CosmereQuestBuilder.TryBuild.
    /// </summary>
    protected override bool CanFireNowSub(IncidentParms parms) {
        if (!base.CanFireNowSub(parms)) return false;
        if (!(parms.target is Verse.Map map)) return false;

        CosmereQuestManager? manager = Current.Game?.GetComponent<CosmereQuestManager>();
        return manager != null && manager.PickWeighted(map) != null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms) {
        if (!(parms.target is Verse.Map map)) return false;

        CosmereQuestManager? manager = Current.Game?.GetComponent<CosmereQuestManager>();
        if (manager == null) return false;

        CosmereQuestDef? questDef = manager.PickWeighted(map);
        if (questDef == null) return false;

        if (!CosmereQuestBuilder.TryBuild(questDef, map, null, out RimWorld.Quest? quest) || quest == null) {
            return false;
        }

        Find.QuestManager.Add(quest);
        manager.RecordOffered(questDef.defName);

        // QuestManager.Add never notifies the player; vanilla does this in IncidentWorker_GiveQuest_Map.GiveQuest.
        QuestUtility.SendLetterQuestAvailable(quest);

        return true;
    }
}
