using Cosmere.Core;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Settings;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Map;

public class MistsWatcher(Verse.Map map) : MapComponent(map) {
    private const int BaseHour = 19; // 7 PM

    private int lastMistsStartTick = -1;
    private bool mistsActive;
    private int mistsEndTick;
    private int mistsStartTick;
    private int nextMistsStartTick = -1;

    private bool enabled {
        get {
            if (!Mod.enableMists) return false;

            // no cache: this used to cache in one place and not another, so it could disagree with itself
            return FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Mists);
        }
    }

    public override void FinalizeInit() {
        if (!enabled) return;

        base.FinalizeInit();
        ScheduleNextMists(); // In case it was missed on map load
    }

    public override void MapComponentTick() {
        if (!enabled) return;

        int currentTick = Find.TickManager.TicksGame;

        if (!mistsActive && currentTick >= nextMistsStartTick) {
            StartMists(map);
        }

        if (!mistsActive) return;
        if (currentTick < mistsStartTick) return;

        // Every hour: scan for exposed pawns
        if (currentTick % GenDate.TicksPerHour == 0) {
            IReadOnlyList<Pawn> allPawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawns.Count; i++) {
                Pawn pawn = allPawns[i];
                if (!pawn.RaceProps.Humanlike || pawn.Dead || pawn.Position.Roofed(map)) continue;
                if (pawn.IsSnapped()) continue;
                if (!pawn.TryGetComp(out Core.Comp.Thing.DormantConnection dormantConnection) ||
                    !dormantConnection.hasDormantConnections) {
                    continue;
                }

                if (!Rand.Chance(1f / MistPressure.OneIn)) continue;

                SnapUtility.Snap(pawn, "the mists");
                pawn.health.AddHediff(HediffDefOf.Cosmere_Scadrial_Hediff_MistComa).Severity = 1.0f;
            }
        }

        if (currentTick < mistsEndTick) return;
        mistsActive = false;
        map.weatherDecider.StartNextWeather();
        ScheduleNextMists();
        Messages.Message("CS_MistsRetreat".Translate(), MessageTypeDefOf.PositiveEvent);
    }

    private void StartMists(Verse.Map map) {
        mistsActive = true;
        int currentTick = Find.TickManager.TicksGame;
        lastMistsStartTick = currentTick;
        mistsStartTick = currentTick + GenDate.TicksPerHour; // 1 hour delay (21:00)
        mistsEndTick = mistsStartTick + GenDate.TicksPerHour * 12; // Lasts until 06:00

        map.weatherManager.TransitionTo(WeatherDefOf.Cosmere_Scadrial_Weather_MistsWeather);

        // Nightly by default, which is a lot of letters once the mists are routine.
        if (Mod.Settings.mistsArrivalLetter) {
            Find.LetterStack.ReceiveLetter(
                "CS_MistsArriveTitle".Translate(),
                "CS_MistsArriveMessage".Translate(),
                LetterDefOf.ThreatSmall,
                TargetInfo.Invalid
            );
        }
    }

    private void ScheduleNextMists() {
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Mists)) {
            return;
        }

        int intervalTicks = GetIntervalTicks();
        int ticksNow = Find.TickManager.TicksGame;

        // If this is the first run, anchor to tonight at baseHour
        if (lastMistsStartTick < 0) {
            int hoursUntilBaseHour = (BaseHour - GenLocalDate.HourOfDay(map) + 24) % 24;
            int ticksUntilBaseHour = hoursUntilBaseHour * GenDate.TicksPerHour;
            lastMistsStartTick = ticksNow + ticksUntilBaseHour - intervalTicks; // So next is tonight
        }

        nextMistsStartTick = lastMistsStartTick + intervalTicks;

        Logger.Verbose(
            $"Next scheduled at tick {nextMistsStartTick} (interval {intervalTicks}, last at {lastMistsStartTick})"
        );
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref mistsActive, "mistsActive");
        Scribe_Values.Look(ref nextMistsStartTick, "nextMistsStartTick");
        Scribe_Values.Look(ref mistsStartTick, "mistsStartTick");
        Scribe_Values.Look(ref mistsEndTick, "mistsEndTick");
        Scribe_Values.Look(ref lastMistsStartTick, "lastMistsStartTick", -1);
    }

    private int GetIntervalTicks() {
        return Mod.mistsFrequency switch {
            MistsFrequency.Daily => GenDate.TicksPerDay,
            MistsFrequency.Weekly => 7 * GenDate.TicksPerDay,
            MistsFrequency.Monthly => 30 * GenDate.TicksPerDay,
            _ => GenDate.TicksPerDay,
        };
    }
}
