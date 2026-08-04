using System;
using Cosmere.Core.ScenarioPart.Parts;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class GameComponent_ScenarioProgression : GameComponent {
    private const int CheckInterval = 250;
    private ScenarioProgressionDef? activeDef;
    private HashSet<string> firedEvents = [];
    private int lastCheckTick = -1;
    private Dictionary<string, int> lastFireTicks = new Dictionary<string, int>();

    public GameComponent_ScenarioProgression(Game game) { }

    public override void StartedNewGame() {
        base.StartedNewGame();
        FindActiveProgression();
    }

    public override void LoadedGame() {
        base.LoadedGame();
        FindActiveProgression();
    }

    private void FindActiveProgression() {
        Scenario? scenario = Find.Scenario;
        if (scenario == null) return;

        foreach (ScenPart part in scenario.AllParts) {
            if (part is ScenPart_ScenarioProgression progressionPart) {
                activeDef = progressionPart.progression;
                return;
            }
        }
    }

    public override void GameComponentTick() {
        base.GameComponentTick();
        if (activeDef == null) return;

        int ticksGame = Find.TickManager.TicksGame;
        if (ticksGame - lastCheckTick < CheckInterval) return;
        lastCheckTick = ticksGame;

        CheckProgression();
    }

    private void CheckProgression() {
        for (int i = 0; i < activeDef!.events.Count; i++) {
            ProgressionEvent evt = activeDef.events[i];

            if (firedEvents.Contains(evt.key) && !evt.repeatable) continue;

            if (evt.repeatable && lastFireTicks.TryGetValue(evt.key, out int lastTick)) {
                int intervalTicks = evt.repeatIntervalDays * GenDate.TicksPerDay;
                if (Find.TickManager.TicksGame - lastTick < intervalTicks) continue;
            }

            bool allMet = true;
            for (int t = 0; t < evt.triggers.Count; t++) {
                if (!evt.triggers[t].IsMet(this)) {
                    allMet = false;
                    break;
                }
            }

            if (!allMet) continue;

            ExecuteEvent(evt);
        }
    }

    private void ExecuteEvent(ProgressionEvent evt) {
        firedEvents.Add(evt.key);
        lastFireTicks[evt.key] = Find.TickManager.TicksGame;

        for (int i = 0; i < evt.actions.Count; i++) {
            try {
                evt.actions[i].Execute(this);
            } catch (Exception ex) {
                Logger.Warning($"ScenarioProgression: Failed to execute action for event '${evt.key}': {ex}");
            }
        }
    }

    public Pawn? FindPawnByName(string firstName) {
        List<Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            List<Pawn> colonists = maps[m].mapPawns.FreeColonists;
            for (int i = 0; i < colonists.Count; i++) {
                if (NameMatches(colonists[i], firstName)) return colonists[i];
            }
        }

        // Caravans too: a pawn who happens to be away on a trade run when a story beat lands is
        // still one of yours, and a beat that skipped him for it would look like a bug.
        List<RimWorld.Planet.Caravan> caravans = Find.WorldObjects.Caravans;
        for (int c = 0; c < caravans.Count; c++) {
            if (!caravans[c].IsPlayerControlled) continue;

            List<Pawn> members = caravans[c].PawnsListForReading;
            for (int i = 0; i < members.Count; i++) {
                if (members[i].IsColonist && NameMatches(members[i], firstName)) return members[i];
            }
        }

        return null;
    }

    private static bool NameMatches(Pawn pawn, string firstName) {
        if (pawn.Name is NameTriple triple) return triple.First == firstName;
        if (pawn.Name is NameSingle single) return single.Name.StartsWith(firstName);
        return false;
    }

    public bool HasEventFired(string eventKey) {
        return firedEvents.Contains(eventKey);
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref firedEvents, "firedEvents", LookMode.Value);
        Scribe_Collections.Look(ref lastFireTicks, "lastFireTicks", LookMode.Value, LookMode.Value);
        firedEvents ??= [];
        lastFireTicks ??= new Dictionary<string, int>();
    }
}
