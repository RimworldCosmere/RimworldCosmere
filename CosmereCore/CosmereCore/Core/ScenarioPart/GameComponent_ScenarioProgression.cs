using System;
using Cosmere.Core.ScenarioPart.Action;
using Cosmere.Core.ScenarioPart.Parts;
using Cosmere.Core.UI;
using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class GameComponent_ScenarioProgression : GameComponent {
    private const int CheckInterval = 250;
    private ScenarioProgressionDef? activeDef;
    private ScenarioProgressionDef? handedOffDef;
    private HashSet<string> firedEvents = [];
    private int lastCheckTick = -1;
    private Dictionary<string, int> lastFireTicks = new Dictionary<string, int>();

    /// <summary>When the running arc took over. Negative while the campaign is on its first.</summary>
    private int arcStartTick = -1;

    /// <summary>Rebuilds the open fork. Non-null means the campaign is waiting on an answer.</summary>
    private Func<Dialog_ProgressionChoice>? pendingChoice;

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
        // An arc the story handed off to wins over the scenario's own (e.g. Final Empire to Well of Ascension).
        if (handedOffDef != null) {
            activeDef = handedOffDef;
            return;
        }

        Scenario? scenario = Find.Scenario;
        if (scenario == null) return;

        foreach (ScenPart part in scenario.AllParts) {
            if (part is ScenPart_ScenarioProgression progressionPart) {
                activeDef = progressionPart.progression;
                return;
            }
        }
    }

    /// <summary>
    ///     Hands the campaign over to the next arc. Fired events are kept: keys are unique per
    ///     arc, and an EventOccurredTrigger in the new arc may want to ask about the old one.
    /// </summary>
    public void HandOffTo(ScenarioProgressionDef next) {
        handedOffDef = next;
        activeDef = next;
        arcStartTick = Find.TickManager.TicksGame;
        Log.Info($"Scenario progression handed off to {next.defName} on day {GenDate.DaysPassed}.");
    }

    /// <summary>
    ///     Days since the running arc took over, rather than since the campaign began. An arc
    ///     entered part-way through a long game has every absolute day threshold already behind
    ///     it, so without this its whole run of events fires in one tick.
    /// </summary>
    public int DaysInActiveArc {
        get {
            if (arcStartTick < 0) return GenDate.DaysPassed;
            return (Find.TickManager.TicksGame - arcStartTick) / GenDate.TicksPerDay;
        }
    }

    public override void GameComponentTick() {
        base.GameComponentTick();

        // A story fork the player hasn't answered outranks everything else; a closed window goes straight back.
        if (pendingChoice != null) {
            if (!Find.WindowStack.IsOpen<Dialog_ProgressionChoice>()) {
                Find.WindowStack.Add(pendingChoice());
            }

            return;
        }

        if (activeDef == null) return;

        int ticksGame = Find.TickManager.TicksGame;
        if (ticksGame - lastCheckTick < CheckInterval) return;
        lastCheckTick = ticksGame;

        CheckProgression();
    }

    /// <summary>
    ///     Puts a fork on screen and holds the campaign there. Nothing else in the arc advances
    ///     until <see cref="ChoiceAnswered" /> is called.
    /// </summary>
    public void AskChoice(Func<Dialog_ProgressionChoice> factory) {
        pendingChoice = factory;
        Find.WindowStack.Add(factory());
    }

    /// <summary>Releases the campaign once a branch has been taken.</summary>
    public void ChoiceAnswered() {
        pendingChoice = null;
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

        RunActions(evt.actions);
    }

    /// <summary>
    ///     Runs a block of actions with their combined effect summary available to any letter
    ///     among them. Choice branches come through here too, so a card only ever lists the
    ///     outcomes of the branch the player actually took.
    /// </summary>
    public void RunActions(List<ProgressionAction> actions) {
        string? previous = PendingEffects;
        PendingEffects = Summarise(actions);

        try {
            for (int i = 0; i < actions.Count; i++) {
                // A fork stops the block: actions after it must not run while the choice is still open.
                if (actions[i] is ChoiceAction choice) {
                    List<ProgressionAction> tail = actions.GetRange(i + 1, actions.Count - i - 1);
                    string? outer = PendingEffects;
                    choice.Execute(this, tail.Count == 0 ? null : () => {
                        string? saved = PendingEffects;
                        PendingEffects = outer;
                        try {
                            RunActions(tail);
                        } finally {
                            PendingEffects = saved;
                        }
                    });
                    return;
                }

                try {
                    actions[i].Execute(this);
                } catch (Exception ex) {
                    Log.Warn($"ScenarioProgression: Failed to execute action: {ex}");
                }
            }
        } finally {
            PendingEffects = previous;
        }
    }

    /// <summary>The mechanical outcomes of the block currently running, one per line.</summary>
    public string? PendingEffects { get; private set; }

    private static string? Summarise(List<ProgressionAction> actions) {
        List<string> lines = new List<string>();
        for (int i = 0; i < actions.Count; i++) {
            string? line = actions[i].Describe();
            if (line != null && line.Length > 0) lines.Add(line);
        }

        return lines.Count == 0 ? null : string.Join("\n", lines);
    }

    public Pawn? FindPawnByName(string firstName) {
        List<Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            List<Pawn> colonists = maps[m].mapPawns.FreeColonists;
            for (int i = 0; i < colonists.Count; i++) {
                if (NameMatches(colonists[i], firstName)) return colonists[i];
            }
        }

        // Caravans too: a pawn away on a trade run is still one of yours.
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

    /// <summary>
    ///     Matches on the nickname as well as the first name. Story beats name people the way
    ///     the books do - Spook, Ham, Breeze - while the pawn carries the real name underneath,
    ///     and a beat written for Spook must still find Lestibournes.
    /// </summary>
    private static bool NameMatches(Pawn pawn, string name) {
        if (pawn.Name is NameTriple triple) {
            return triple.First == name || triple.Nick == name || triple.Last == name;
        }

        if (pawn.Name is NameSingle single) return single.Name.StartsWith(name);
        return false;
    }

    public bool HasEventFired(string eventKey) {
        return firedEvents.Contains(eventKey);
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref handedOffDef, "handedOffDef");
        Scribe_Values.Look(ref arcStartTick, "arcStartTick", -1);
        Scribe_Collections.Look(ref firedEvents, "firedEvents", LookMode.Value);
        Scribe_Collections.Look(ref lastFireTicks, "lastFireTicks", LookMode.Value, LookMode.Value);
        firedEvents ??= [];
        lastFireTicks ??= new Dictionary<string, int>();
    }
}
