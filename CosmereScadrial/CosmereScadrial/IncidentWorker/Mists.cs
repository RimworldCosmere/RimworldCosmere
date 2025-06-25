using System.Linq;
using CosmereFramework.Util;
using CosmereScadrial.Util;
using RimWorld;
using Verse;

// @todo Delete
namespace CosmereScadrial.IncidentWorker;

public class Mists : RimWorld.IncidentWorker {
    protected override bool CanFireNowSub(IncidentParms parms) {
        return parms.target is Map map && GenLocalDate.HourOfDay(map) == 20;
    }

    protected override bool TryExecuteWorker(IncidentParms parms) {
        if (parms.target is not Map map) {
            return false;
        }

        // Start the 1-hour delay (2500 ticks)
        DelayedActionScheduler.Schedule(() => {
            int snappedCount = 0;

            foreach (Pawn? pawn in map.mapPawns.AllPawnsSpawned.Where(p =>
                         p.RaceProps.Humanlike && !p.Dead && !p.Position.Roofed(map))) {
                if (SnapUtility.IsSnapped(pawn)) continue;
                // Check if the pawn has DormantConnection
                SnapUtility.TrySnap(pawn);
                pawn.health.AddHediff(HediffDefOf.Cosmere_Scadrial_Hediff_MistComa).Severity = 1.0f;
                snappedCount++;
            }

            Find.LetterStack.ReceiveLetter(
                "The mists have arrived",
                snappedCount > 0
                    ? $"The mists rolled through the colony, and {snappedCount} pawn(s) were forever changed."
                    : "The mists rolled through the colony. This time, they passed without consequence.",
                LetterDefOf.NegativeEvent,
                new TargetInfo(map.Center, map)
            );
        }, 2500);

        return true;
    }
}