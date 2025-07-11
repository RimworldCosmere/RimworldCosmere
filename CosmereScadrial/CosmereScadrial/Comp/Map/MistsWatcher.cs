using System.Linq;
using CosmereCore.Util;
using CosmereFramework;
using CosmereScadrial.Settings;
using CosmereScadrial.Util;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comp.Map;

public class MistsWatcher(Verse.Map map) : MapComponent(map) {
    private const int BaseHour = 19; // 7 PM
    private int lastMistsStartTick = -1;
    private bool mistsActive;
    private int mistsEndTick;
    private int mistsStartTick;
    private int nextMistsStartTick = -1;

    private static bool enabled { get; } =
        ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Preservation, ShardDefOf.Harmony);

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
            foreach (Pawn? pawn in map.mapPawns.AllPawnsSpawned.Where(p =>
                         p.RaceProps.Humanlike && !p.Dead && !p.Position.Roofed(map)
                     )) {
                if (SnapUtility.IsSnapped(pawn)) continue;
                if (!pawn.genes.HasActiveGene(GeneDefOf.Cosmere_Scadrial_Gene_DormantMetalborn)) continue;
                if (!Rand.Chance(1f / 16f)) continue;

                SnapUtility.TrySnap(pawn, "the mists");
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

        Find.LetterStack.ReceiveLetter(
            "CS_MistsArriveTitle".Translate(),
            "CS_MistsArriveMessage".Translate(),
            LetterDefOf.ThreatSmall,
            TargetInfo.Invalid
        );
    }

    private void ScheduleNextMists() {
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Preservation, ShardDefOf.Harmony)) {
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
            $"[Mists] Next scheduled at tick {nextMistsStartTick} (interval {intervalTicks}, last at {lastMistsStartTick})"
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
        return CosmereScadrial.mistsFrequency switch {
            MistsFrequency.Daily => GenDate.TicksPerDay,
            MistsFrequency.Weekly => 7 * GenDate.TicksPerDay,
            MistsFrequency.Monthly => 30 * GenDate.TicksPerDay,
            _ => GenDate.TicksPerDay,
        };
    }
}