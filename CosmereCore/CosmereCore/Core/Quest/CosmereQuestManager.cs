using System;
using System.Text;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quest;

/// <summary>
///     Owns all persisted quest state and wires the pure logic in CosmereQuestEligibility and
///     CapstoneStateMachine to live Verse state (Find, DefDatabase, Scribe). This class has no
///     tests of its own: it is a thin shell that gathers state and delegates every decision to
///     CosmereQuestEligibility and CapstoneStateMachine, both of which are already unit-tested.
/// </summary>
public class CosmereQuestManager : GameComponent {
    private Dictionary<string, CapstoneState> capstoneStates = new Dictionary<string, CapstoneState>();
    private Dictionary<string, HashSet<int>> pawnBurns = new Dictionary<string, HashSet<int>>();
    private HashSet<string> flags = new HashSet<string>();

    /// <summary>
    ///     The era the campaign has reached, once the story has moved it on. Null until then,
    ///     when the scenario's own declared era stands.
    /// </summary>
    public string? currentEra;
    private Dictionary<string, int> lastOfferedTick = new Dictionary<string, int>();

    /// <summary>
    ///     pawnBurns pairs a quest defName with the thingIDNumbers of pawns that have burned that
    ///     capstone for that quest. HashSet&lt;int&gt; is neither IExposable nor a scribable
    ///     primitive, so it cannot be a Scribe_Collections.Look dictionary value: LookMode.Deep
    ///     silently drops it (logs an error, writes nothing) and LookMode.Value serializes it as
    ///     unparseable ToString() text. Shards.cs solves the same class of problem for
    ///     Dictionary&lt;string, Shard&gt; (Shard is not IExposable either) by keeping a flattened
    ///     shadow list that is rebuilt on PostLoadInit. These two fields are that same shadow,
    ///     used only for serialization; pawnBurns above remains the real, authoritative field.
    /// </summary>
    private List<string> savedPawnBurnDefNames = new List<string>();

    private List<int> savedPawnBurnPawnIds = new List<int>();

    public CosmereQuestManager(Verse.Game game) {
        QuestFlagStore.SetFlag = SetFlag;
        QuestFlagStore.HasFlag = HasFlag;
    }

    public QuestWorldState BuildWorldState() {
        Cosmere.Core.Def.CosmereWorldDef? world = WorldUtility.Primary;

        QuestWorldState state = new QuestWorldState {
            currentTick = Find.TickManager.TicksGame,
            daysElapsed = GenDate.DaysPassed,
            era = FindActiveEra(),
            world = world?.defName,
            crossWorld = world?.crossWorld ?? false,
        };

        foreach (KeyValuePair<string, CapstoneState> pair in capstoneStates) {
            state.capstoneStates[pair.Key] = pair.Value;
            if (pair.Value == CapstoneState.Completed) {
                state.completedCapstones.Add(pair.Key);
            }
        }

        foreach (KeyValuePair<string, int> pair in lastOfferedTick) {
            state.lastOfferedTick[pair.Key] = pair.Value;
        }

        foreach (string flag in flags) {
            state.flags.Add(flag);
        }

        Cosmere.Core.Comp.Game.Shards? shards = ShardUtility.shards;
        if (shards != null) {
            foreach (string shardDefName in shards.enabledShards.Keys) {
                state.enabledShards.Add(shardDefName);
            }
        }

        List<Verse.Map> maps = Find.Maps;
        int colonistCount = 0;
        for (int i = 0; i < maps.Count; i++) {
            colonistCount += maps[i].mapPawns.FreeColonistsSpawnedCount;
        }

        state.freeColonistCount = colonistCount;

        List<Faction> factions = Find.FactionManager.AllFactionsListForReading;
        for (int i = 0; i < factions.Count; i++) {
            FactionDef? def = factions[i].def;
            if (def != null) state.presentFactions.Add(def.defName);
        }

        return state;
    }

    /// <summary>
    ///     Picks a Threat that may arrive right now, weighted like the offer pool. Separate from
    ///     PickWeighted because Threats are excluded from the storyteller's offers by design.
    /// </summary>
    public CosmereQuestDef? PickThreat(Verse.Map map) {
        QuestWorldState state = BuildWorldState();

        List<CosmereQuestDef> defs = DefDatabase<CosmereQuestDef>.AllDefsListForReading;
        List<CosmereQuestDef> eligible = new List<CosmereQuestDef>();
        float totalWeight = 0f;
        for (int i = 0; i < defs.Count; i++) {
            QuestCandidate candidate = defs[i].ToCandidate();
            if (!CosmereQuestEligibility.IsDeliverableAsThreat(candidate, state)) continue;

            eligible.Add(defs[i]);
            totalWeight += CosmereQuestEligibility.WeightOf(candidate);
        }

        if (eligible.Count == 0 || totalWeight <= 0f) return null;

        float roll = Rand.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < eligible.Count; i++) {
            cumulative += CosmereQuestEligibility.WeightOf(eligible[i].ToCandidate());
            if (roll <= cumulative) return eligible[i];
        }

        return eligible[eligible.Count - 1];
    }

    public CosmereQuestDef? PickWeighted(Verse.Map map) {
        QuestWorldState state = BuildWorldState();

        List<CosmereQuestDef> defs = DefDatabase<CosmereQuestDef>.AllDefsListForReading;
        List<QuestCandidate> candidates = new List<QuestCandidate>(defs.Count);
        for (int i = 0; i < defs.Count; i++) {
            candidates.Add(defs[i].ToCandidate());
        }

        List<QuestCandidate> eligible = CosmereQuestEligibility.Filter(candidates, state);
        if (eligible.Count == 0) return null;

        LogPool(candidates, eligible, state);

        float totalWeight = 0f;
        for (int i = 0; i < eligible.Count; i++) {
            totalWeight += CosmereQuestEligibility.WeightOf(eligible[i]);
        }

        if (totalWeight <= 0f) return null;

        float roll = Rand.Range(0f, totalWeight);
        float cumulative = 0f;
        string? pickedName = null;
        for (int i = 0; i < eligible.Count; i++) {
            cumulative += CosmereQuestEligibility.WeightOf(eligible[i]);
            if (roll <= cumulative) {
                pickedName = eligible[i].defName;
                break;
            }
        }

        pickedName ??= eligible[eligible.Count - 1].defName;

        for (int i = 0; i < defs.Count; i++) {
            if (defs[i].defName == pickedName) return defs[i];
        }

        return null;
    }

    /// <summary>
    ///     Says which quests were in the pool and which were filtered out. Without this, a
    ///     def-level gate like minDaysElapsed looks exactly like bad luck - the same quest keeps
    ///     coming up and the other one appears to be broken.
    /// </summary>
    private static void LogPool(
        List<QuestCandidate> candidates,
        List<QuestCandidate> eligible,
        QuestWorldState state
    ) {
        StringBuilder builder = new StringBuilder();
        builder.Append($"Quest pool at day {state.daysElapsed}, era {state.era ?? "none"}: ");
        for (int i = 0; i < candidates.Count; i++) {
            QuestCandidate candidate = candidates[i];
            bool passed = false;
            for (int j = 0; j < eligible.Count; j++) {
                if (eligible[j].defName == candidate.defName) {
                    passed = true;
                    break;
                }
            }

            builder.Append(candidate.defName);
            builder.Append(passed ? " [eligible] " : " [filtered] ");
        }

        Logger.Verbose(builder.ToString());
    }

    public bool TryStartCapstone(CosmereQuestDef def, Verse.Map map, Pawn? subject) {
        CapstoneState current = capstoneStates.TryGetValue(def.defName, out CapstoneState existing)
            ? existing
            : CapstoneState.NotFired;
        if (!CapstoneStateMachine.CanTransition(current, CapstoneState.Offered)) {
            Logger.Warning($"{def.defName}: cannot start capstone from state {current}.");
            return false;
        }

        if (!CosmereQuestBuilder.TryBuild(def, map, subject, out RimWorld.Quest? quest) || quest == null) {
            return false;
        }

        Find.QuestManager.Add(quest);
        QuestUtility.SendLetterQuestAvailable(quest);
        RecordOffered(def.defName);
        capstoneStates[def.defName] = CapstoneState.Offered;
        return true;
    }

    /// <summary>
    ///     Starts a Threat quest already accepted. A Threat is not an offer - it arrives and the
    ///     player deals with it - so there is no accept/dec‍line letter and no acceptance deadline.
    /// </summary>
    public bool TryStartThreat(CosmereQuestDef def, Verse.Map map, Pawn? subject) {
        if (def.kind != QuestKind.Threat) {
            Logger.Error($"{def.defName}: TryStartThreat called on a {def.kind} quest.");
            return false;
        }

        if (!CosmereQuestBuilder.TryBuild(def, map, subject, out RimWorld.Quest? quest) || quest == null) {
            return false;
        }

        // Call before Add: Add() calls Initiate() for an already-accepted quest, enabling the first stage's parts.
        quest.SetInitiallyAccepted();

        Find.QuestManager.Add(quest);
        QuestUtility.SendLetterQuestAvailable(quest);
        RecordOffered(def.defName);
        return true;
    }

    public void NotifyDeclined(string defName) {
        CapstoneState current = capstoneStates.TryGetValue(defName, out CapstoneState existing)
            ? existing
            : CapstoneState.NotFired;
        capstoneStates[defName] = CapstoneStateMachine.OnDeclined(current);
    }

    public void NotifyCompleted(string defName) {
        CapstoneState current = capstoneStates.TryGetValue(defName, out CapstoneState existing)
            ? existing
            : CapstoneState.NotFired;
        capstoneStates[defName] = CapstoneStateMachine.OnCompleted(current);
    }

    public void NotifyFailed(string defName, Pawn? subject) {
        if (subject == null) {
            CapstoneState current = capstoneStates.TryGetValue(defName, out CapstoneState existing)
                ? existing
                : CapstoneState.NotFired;
            capstoneStates[defName] = CapstoneStateMachine.OnFailed(current);
            return;
        }

        if (!pawnBurns.TryGetValue(defName, out HashSet<int>? burnedPawns)) {
            burnedPawns = new HashSet<int>();
            pawnBurns[defName] = burnedPawns;
        }

        burnedPawns.Add(subject.thingIDNumber);
    }

    public void SetFlag(string flag) {
        flags.Add(flag);
    }

    public bool HasFlag(string flag) {
        return flags.Contains(flag);
    }

    public void RecordOffered(string defName) {
        lastOfferedTick[defName] = Find.TickManager.TicksGame;
    }

    /// <summary>
    ///     Moves the campaign into the era that follows the one it is in. Returns false at the
    ///     end of a shardworld's timeline, where the current era declares no successor.
    /// </summary>
    public bool AdvanceEra() {
        string? active = FindActiveEra();
        if (active == null || active.Length == 0) {
            Logger.Warning("AdvanceEra: this campaign has no era to advance from.");
            return false;
        }

        Cosmere.Core.Def.EraDef? era = DefDatabase<Cosmere.Core.Def.EraDef>.GetNamedSilentFail(active);
        if (era?.next == null) {
            Logger.Info($"AdvanceEra: '{active}' is the last era of its timeline, staying put.");
            return false;
        }

        currentEra = era.next.defName;
        Logger.Important($"The campaign has moved from the {era.label} into the {era.next.label}.");
        return true;
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref currentEra, "currentEra");

        Scribe_Collections.Look(ref capstoneStates, "capstoneStates", LookMode.Value, LookMode.Value);
        capstoneStates ??= new Dictionary<string, CapstoneState>();

        Scribe_Collections.Look(ref flags, "flags", LookMode.Value);
        flags ??= new HashSet<string>();

        Scribe_Collections.Look(ref lastOfferedTick, "lastOfferedTick", LookMode.Value, LookMode.Value);
        lastOfferedTick ??= new Dictionary<string, int>();

        if (Scribe.mode == LoadSaveMode.Saving) {
            savedPawnBurnDefNames.Clear();
            savedPawnBurnPawnIds.Clear();
            foreach (KeyValuePair<string, HashSet<int>> pair in pawnBurns) {
                foreach (int pawnId in pair.Value) {
                    savedPawnBurnDefNames.Add(pair.Key);
                    savedPawnBurnPawnIds.Add(pawnId);
                }
            }
        }

        Scribe_Collections.Look(ref savedPawnBurnDefNames, "pawnBurnDefNames", LookMode.Value);
        Scribe_Collections.Look(ref savedPawnBurnPawnIds, "pawnBurnPawnIds", LookMode.Value);
        savedPawnBurnDefNames ??= new List<string>();
        savedPawnBurnPawnIds ??= new List<int>();

        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            pawnBurns = new Dictionary<string, HashSet<int>>();
            int pairCount = Math.Min(savedPawnBurnDefNames.Count, savedPawnBurnPawnIds.Count);
            for (int i = 0; i < pairCount; i++) {
                string defName = savedPawnBurnDefNames[i];
                if (!pawnBurns.TryGetValue(defName, out HashSet<int>? burnedPawns)) {
                    burnedPawns = new HashSet<int>();
                    pawnBurns[defName] = burnedPawns;
                }

                burnedPawns.Add(savedPawnBurnPawnIds[i]);
            }
        }
    }

    public static string? FindActiveEra() {
        // An era the story has advanced into wins over everything: post-Catacendre isn't the scenario's original age.
        CosmereQuestManager? manager = Current.Game?.GetComponent<CosmereQuestManager>();
        string? reached = manager?.currentEra;
        if (reached != null && reached.Length > 0) return reached;

        // A cross-shard quickstart declares its own era, because its scenario cannot.
        string? forced = Quickstart.Quickstarter.instance?.Quickstart?.era;
        if (forced != null && forced.Length > 0) return forced;

        string? scenarioName = Find.Scenario?.name;
        if (scenarioName == null || scenarioName.Length == 0) return null;

        List<ScenarioDef> defs = DefDatabase<ScenarioDef>.AllDefsListForReading;
        for (int i = 0; i < defs.Count; i++) {
            ScenarioDef def = defs[i];
            if (def.label != scenarioName) continue;

            Cosmere.Core.DefModExtension.ScenarioEra? extension =
                def.GetModExtension<Cosmere.Core.DefModExtension.ScenarioEra>();
            return extension?.era?.defName;
        }

        return null;
    }
}
