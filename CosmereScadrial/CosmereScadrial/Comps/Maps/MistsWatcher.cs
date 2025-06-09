using System.Linq;
using CosmereCore.Utils;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comps.Maps {
    public class MistsWatcher(Map map) : MapComponent(map) {
        private const int BaseHour = 19; // 7 PM
        private int lastMistsStartTick = -1;
        private bool mistsActive;
        private int mistsEndTick;
        private int mistsStartTick;
        private int nextMistsStartTick;

        public override void FinalizeInit() {
            base.FinalizeInit();
            ScheduleNextMists(); // In case it was missed on map load
        }

        public override void MapComponentTick() {
            var currentTick = Find.TickManager.TicksGame;

            if (!mistsActive && currentTick >= nextMistsStartTick) {
                StartMists(map);
            }

            if (!mistsActive) return;
            if (currentTick < mistsStartTick) return;
            // Every hour: scan for exposed pawns
            if (currentTick % GenDate.TicksPerHour == 0) {
                foreach (var pawn in map.mapPawns.AllPawnsSpawned.Where(p =>
                             p.RaceProps.Humanlike && !p.Dead && !p.Position.Roofed(map))) {
                    if (SnapUtility.IsSnapped(pawn)) continue;
                    if (!pawn.genes.HasActiveGene(GeneDefOf.Cosmere_Scadrial_DormantMetalborn)) continue;
                    if (!Rand.Chance(1f / 16f)) continue;

                    SnapUtility.TrySnap(pawn, "the mists");
                    pawn.health.AddHediff(HediffDefOf.Cosmere_Scadrial_MistComa).Severity = 1.0f;
                }
            }

            if (currentTick < mistsEndTick) return;
            mistsActive = false;
            ScheduleNextMists();
            Messages.Message("The mists retreat with the dawn.", MessageTypeDefOf.PositiveEvent);
        }

        private void StartMists(Map map) {
            mistsActive = true;
            var currentTick = Find.TickManager.TicksGame;
            lastMistsStartTick = currentTick;
            mistsStartTick = currentTick + GenDate.TicksPerHour; // 1 hour delay (21:00)
            mistsEndTick = mistsStartTick + GenDate.TicksPerHour * 12; // Lasts until 06:00

            map.weatherManager.TransitionTo(WeatherDefOf.Cosmere_Scadrial_MistsWeather);

            Find.LetterStack.ReceiveLetter(
                "The mists arrive",
                "As night falls, a dense, ethereal mist blankets the land. At 20:00, they will settle. Pawns outside during the night may never be the same.",
                LetterDefOf.ThreatSmall,
                new TargetInfo(map.Center, map)
            );
        }

        private void ScheduleNextMists() {
            if (!ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Preservation, ShardDefOf.Harmony)) {
                return;
            }

            var intervalTicks = GetIntervalTicks();
            var ticksNow = Find.TickManager.TicksGame;

            // If this is the first run, anchor to tonight at baseHour
            if (lastMistsStartTick < 0) {
                var hoursUntilBaseHour = (BaseHour - GenLocalDate.HourOfDay(map) + 24) % 24;
                var ticksUntilBaseHour = hoursUntilBaseHour * GenDate.TicksPerHour;
                lastMistsStartTick = ticksNow + ticksUntilBaseHour - intervalTicks; // So next is tonight
            }

            nextMistsStartTick = lastMistsStartTick + intervalTicks;

            Log.Message(
                $"[Mists] Next scheduled at tick {nextMistsStartTick} (interval {intervalTicks}, last at {lastMistsStartTick})");
        }


        public override void ExposeData() {
            Scribe_Values.Look(ref mistsActive, "mistsActive");
            Scribe_Values.Look(ref nextMistsStartTick, "nextMistsStartTick");
            Scribe_Values.Look(ref mistsStartTick, "mistsStartTick");
            Scribe_Values.Look(ref mistsEndTick, "mistsEndTick");
            Scribe_Values.Look(ref lastMistsStartTick, "lastMistsStartTick", -1);
        }

        private int GetIntervalTicks() {
            return CosmereScadrial.Settings.mistsFrequency switch {
                MistsFrequency.Daily => GenDate.TicksPerDay,
                MistsFrequency.Weekly => 7 * GenDate.TicksPerDay,
                MistsFrequency.Monthly => 30 * GenDate.TicksPerDay,
                _ => GenDate.TicksPerDay,
            };
        }
    }
}