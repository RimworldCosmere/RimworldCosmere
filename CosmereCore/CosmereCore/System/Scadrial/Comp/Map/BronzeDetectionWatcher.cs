using System.Text;
using Cosmere.System.Scadrial.Utility;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Map;

public class BronzeDetectionWatcher(Verse.Map map) : MapComponent(map) {
    private const int ScanIntervalTicks = 2500;
    private bool hasReportedCurrentTile;
    private HashSet<int> reportedTiles = [];
    private int tickCounter;

    public override void MapComponentTick() {
        tickCounter++;
        if (tickCounter < ScanIntervalTicks) return;
        tickCounter = 0;

        float strength = AllomancyUtility.GetBronzeSeekerStrength(map);
        if (strength <= 0f) return;

        if (!hasReportedCurrentTile) {
            ReportCurrentTile();
            hasReportedCurrentTile = true;
        }

        ReportAdjacentTiles(strength);
    }

    private void ReportCurrentTile() {
        string? report = BronzeDetectionUtility.GetTileResourceReport(map.Tile);
        if (report == null) return;

        Messages.Message(
            "Cosmere_Scadrial_BronzeDetection_Colony".Translate(report),
            MessageTypeDefOf.PositiveEvent,
            false
        );
    }

    private void ReportAdjacentTiles(float strength) {
        int scanDepth = strength >= 1.5f ? 3 : strength >= 0.75f ? 2 : 1;
        List<int> tilesToScan = BronzeDetectionUtility.GetTilesInRange(map.Tile, scanDepth);
        StringBuilder newFinds = new StringBuilder();
        int newTileCount = 0;

        for (int i = 0; i < tilesToScan.Count; i++) {
            int tileId = tilesToScan[i];
            if (!reportedTiles.Add(tileId)) continue;

            string? report = BronzeDetectionUtility.GetTileResourceReport(tileId);
            if (report == null) continue;

            if (newFinds.Length > 0) newFinds.Append('\n');
            newFinds.Append("Cosmere_Scadrial_BronzeDetection_AdjacentEntry".Translate(tileId, report));
            newTileCount++;
        }

        if (newTileCount <= 0) return;

        Find.LetterStack.ReceiveLetter(
            "Cosmere_Scadrial_BronzeDetection_Title".Translate(),
            "Cosmere_Scadrial_BronzeDetection_Adjacent".Translate(newTileCount, newFinds.ToString()),
            LetterDefOf.PositiveEvent,
            new GlobalTargetInfo(new PlanetTile(map.Tile))
        );
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref reportedTiles, "reportedTiles", LookMode.Value);
        Scribe_Values.Look(ref hasReportedCurrentTile, "hasReportedCurrentTile");
        Scribe_Values.Look(ref tickCounter, "tickCounter");
        reportedTiles ??= [];
    }
}