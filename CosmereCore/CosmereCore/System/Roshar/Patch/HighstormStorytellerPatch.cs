using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch;

// TODO This could probably be a Component (Map or Game), instead of a patch. Or eventually, Trigger by the actual storm that's moving through the world
[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.StorytellerTick))]
public static class HighstormStorytellerPatch {
    private const int IntervalTicks = 8 * 60000; // 1 in-game days
    private const int WarningOffsetTicks = 60000; // Half a day (0.5 * 60000 ticks)

    [HarmonyPostfix]
    public static void StorytellerTick_Postfix() {
        int currentTick = Find.TickManager.TicksGame;

        if (currentTick % IntervalTicks == IntervalTicks - WarningOffsetTicks) {
            ShowWarning();
        }

        if (currentTick % IntervalTicks == 0) {
            TryTriggerHighstorm();
        }
    }

    private static void TryTriggerHighstorm() {
        if (Find.CurrentMap == null) return; // Ensure a valid map exists

        IncidentParms parms = new IncidentParms {
            target = Find.CurrentMap, // Ensure the incident happens in the current map
            forced = true, // Forces it to trigger even if it has a low chance
        };

        bool success = Find.Storyteller.incidentQueue.Add(
            DefDatabase<IncidentDef>.GetNamed("Cosmere_Roshar_HighstormIncident"),
            Find.TickManager.TicksGame,
            parms
        );

        if (success) {
            Log.Message("[Cosmere.System.Roshar] Highstorm triggered!");
        }
    }


    private static void ShowWarning() {
        if (Find.CurrentMap == null) return; // Ensure a valid map exists

        // Show a warning message to the player
        Find.LetterStack.ReceiveLetter(
            "Approaching Highstorm",
            "A Highstorm is approaching! Seek shelter immediately. The storm will arrive in a day.",
            RimWorld.LetterDefOf.NeutralEvent,
            new LookTargets(Find.CurrentMap.mapPawns.FreeColonists.RandomElement()) // Random colonist as a reference
        );
    }
}