using Concord;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Allomancy;

[Patch]
public abstract class CaravanBronzeDetectionPatch : Caravan {
    private static readonly Dictionary<int, int> lastKnownTile = [];

    [Inject(At.Return, nameof(TickInterval))]
    private void AfterTickInterval() {
        Caravan self = this;
        if (!self.IsPlayerControlled) return;

        int caravanId = self.ID;
        int currentTile = self.Tile;

        bool isFirstSeen = !lastKnownTile.TryGetValue(caravanId, out int previousTile);
        lastKnownTile[caravanId] = currentTile;

        if (isFirstSeen || previousTile == currentTile) return;

        List<Pawn> pawns = self.PawnsListForReading;
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
