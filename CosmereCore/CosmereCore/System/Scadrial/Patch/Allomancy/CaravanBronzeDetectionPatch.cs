using HarmonyLib;
using Verse;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using RimWorld.Planet;

namespace Cosmere.System.Scadrial.Patch.Allomancy;

[HarmonyPatch(typeof(Caravan), "TickInterval")]
public static class CaravanBronzeDetectionPatch {
    private static readonly Dictionary<int, int> lastKnownTile = [];

    [HarmonyPostfix]
    public static void Postfix(Caravan __instance) {
        if (!__instance.IsPlayerControlled) return;

        int caravanId = __instance.ID;
        int currentTile = __instance.Tile;

        bool isFirstSeen = !lastKnownTile.TryGetValue(caravanId, out int previousTile);
        lastKnownTile[caravanId] = currentTile;

        if (isFirstSeen || previousTile == currentTile) return;

        List<Pawn> pawns = __instance.PawnsListForReading;
        if (!AllomancyUtility.CaravanHasActiveBronzeSeeker(pawns)) return;

        string? resourceReport = BronzeDetectionUtility.GetTileResourceReport(currentTile);
        if (resourceReport == null) return;

        Find.LetterStack.ReceiveLetter(
            "Cosmere_Scadrial_BronzeDetection_Title".Translate(),
            "Cosmere_Scadrial_BronzeDetection_Caravan".Translate(resourceReport),
            LetterDefOf.PositiveEvent,
            new GlobalTargetInfo(new PlanetTile(currentTile))
        );
    }
}