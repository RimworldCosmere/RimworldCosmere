using System.Linq;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comps.Maps {
    public class MistsWatcher(Map map) : MapComponent(map) {
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
            // Every 100 ticks: scan for exposed pawns
            if (currentTick % 100 == 0) {
                foreach (var pawn in map.mapPawns.AllPawnsSpawned.Where(p =>
                             p.RaceProps.Humanlike && !p.Dead && !p.Position.Roofed(map))) {
                    if (SnapUtility.IsSnapped(pawn)) continue;

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
            mistsStartTick = currentTick + GenDate.TicksPerHour; // 1 hour delay (21:00)
            mistsEndTick = mistsStartTick + GenDate.TicksPerHour * 9; // Lasts until 06:00

            map.weatherManager.TransitionTo(WeatherDefOf.Cosmere_Scadrial_MistsWeather);
            Find.LetterStack.ReceiveLetter(
                "The mists arrive",
                "As night falls, a dense, ethereal mist blankets the land. At 21:00, they will settle. Pawns outside during the night may never be the same.",
                LetterDefOf.NegativeEvent,
                new TargetInfo(map.Center, map)
            );
        }

        private void ScheduleNextMists() {
            var currentHour = GenLocalDate.HourOfDay(map);
            var ticksUntil20 = (20 - currentHour + 24) % 24 * GenDate.TicksPerHour;
            nextMistsStartTick = Find.TickManager.TicksGame + ticksUntil20;
        }

        public override void ExposeData() {
            Scribe_Values.Look(ref mistsActive, "mistsActive");
            Scribe_Values.Look(ref nextMistsStartTick, "nextMistsStartTick");
            Scribe_Values.Look(ref mistsStartTick, "mistsStartTick");
            Scribe_Values.Look(ref mistsEndTick, "mistsEndTick");
        }
    }
}