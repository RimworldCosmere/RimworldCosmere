using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class GameComponent_ScenarioProgression : GameComponent {
    private ScenarioProgressionDef? activeDef;
    private HashSet<string> firedEvents = [];
    private Dictionary<string, int> lastFireTicks = new();
    private int lastCheckTick = -1;
    private const int CheckInterval = 250;

    public GameComponent_ScenarioProgression(Verse.Game game) { }

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
            } catch (global::System.Exception ex) {
                Logger.Warning($"ScenarioProgression: Failed to execute action for event '{evt.key}': {ex.Message}");
            }
        }
    }

    public Pawn? FindPawnByName(string firstName) {
        List<Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            List<Pawn> colonists = maps[m].mapPawns.FreeColonists;
            for (int i = 0; i < colonists.Count; i++) {
                Pawn pawn = colonists[i];
                if (pawn.Name is NameTriple triple && triple.First == firstName) return pawn;
                if (pawn.Name is NameSingle single && single.Name.StartsWith(firstName)) return pawn;
            }
        }
        return null;
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
