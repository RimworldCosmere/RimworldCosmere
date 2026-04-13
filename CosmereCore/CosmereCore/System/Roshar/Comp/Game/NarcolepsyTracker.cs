using System.Collections.Generic;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Comp.Game;

public class NarcolepsyTracker : GameComponent {
    private static NarcolepsyTracker? instance;
    public static NarcolepsyTracker? Instance => instance;

    private List<Verse.Pawn> trackedPawns = [];
    private List<int> nextCollapseTicks = [];


    public NarcolepsyTracker(Verse.Game game) {
        instance = this;
    }

    public void Register(Verse.Pawn pawn) {
        if (trackedPawns.Contains(pawn)) return;
        trackedPawns.Add(pawn);
        int nextTick = GenTicks.TicksGame + GenDate.TicksPerDay * 7;
        nextCollapseTicks.Add(nextTick);
        int ticksUntil = nextTick - GenTicks.TicksGame;
        Logger.Info($"NarcolepsyTracker: registered {pawn.NameShortColored}, next collapse in {ticksUntil} ticks ({ticksUntil / (float)GenDate.TicksPerDay:F1} days)");
    }

    public int GetTicksUntilNextCollapse(Verse.Pawn pawn) {
        int idx = trackedPawns.IndexOf(pawn);
        if (idx < 0) return -1;
        return nextCollapseTicks[idx] - GenTicks.TicksGame;
    }

    public override void GameComponentTick() {
        if (GenTicks.TicksGame % 2500 != 0) return;
        HediffDef? collapseDef = HediffDefOf.Cosmere_Roshar_Hediff_NW_NarcolepsyCollapse;
        if (collapseDef == null) return;

        int currentTick = GenTicks.TicksGame;
        for (int i = trackedPawns.Count - 1; i >= 0; i--) {
            Verse.Pawn pawn = trackedPawns[i];
            if (pawn == null || pawn.Dead || pawn.Destroyed) {
                trackedPawns.RemoveAt(i);
                nextCollapseTicks.RemoveAt(i);
                continue;
            }
            if (currentTick < nextCollapseTicks[i]) continue;

            int nextInterval = GenDate.TicksPerDay * 7;
            nextCollapseTicks[i] = currentTick + nextInterval;

            if (!pawn.Spawned) {
                Logger.Verbose($"NarcolepsyTracker: {pawn.NameShortColored} skipped (not spawned), next in {nextInterval / (float)GenDate.TicksPerDay:F1} days");
                continue;
            }
            if (pawn.Downed || pawn.InBed()) {
                Logger.Verbose($"NarcolepsyTracker: {pawn.NameShortColored} skipped (downed/in bed), next in {nextInterval / (float)GenDate.TicksPerDay:F1} days");
                continue;
            }
            if (pawn.health.hediffSet.HasHediff(collapseDef)) {
                Logger.Verbose($"NarcolepsyTracker: {pawn.NameShortColored} skipped (already collapsed), next in {nextInterval / (float)GenDate.TicksPerDay:F1} days");
                continue;
            }

            Logger.Info($"NarcolepsyTracker: {pawn.NameShortColored} collapsing! Next in {nextInterval / (float)GenDate.TicksPerDay:F1} days");
            pawn.health.AddHediff(HediffMaker.MakeHediff(collapseDef, pawn));
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref trackedPawns, "narcolepsyPawns", LookMode.Reference);
        Scribe_Collections.Look(ref nextCollapseTicks, "narcolepsyNextTicks", LookMode.Value);
        trackedPawns ??= [];
        nextCollapseTicks ??= [];
        while (nextCollapseTicks.Count < trackedPawns.Count)
            nextCollapseTicks.Add(GenTicks.TicksGame + GenDate.TicksPerDay * 7);
    }
}
