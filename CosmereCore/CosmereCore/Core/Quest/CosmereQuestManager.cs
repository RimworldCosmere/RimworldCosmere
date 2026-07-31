using System;
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
        QuestWorldState state = new QuestWorldState {
            currentTick = Find.TickManager.TicksGame,
            daysElapsed = GenDate.DaysPassed,
            era = FindActiveEra(),
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

        return state;
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

    public override void ExposeData() {
        base.ExposeData();

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

    private static string? FindActiveEra() {
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
