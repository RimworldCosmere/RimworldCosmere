using Cosmere.System.Scadrial.Comp.Game;
using Cosmere.System.Scadrial.Util;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.MapGen;

/// <summary>
///     Scatters ash vents across a map, more of them the closer the tile sits to an Ashmount.
///     Base class matches the steam geyser so a vent gets the same footprint and terrain checks.
/// </summary>
public class GenStep_ScatterAshVents : GenStep_ScatterThings {
    private const int ElevationSampleRadius = 10;

    public override int SeedPart => 0x5AE17;

    /// <summary>
    ///     Gate lives here, not in the XPath patch. A patched-away genStep list breaks games that
    ///     never wanted the mod's content - the failure shape from 18518fe7.
    /// </summary>
    public override void Generate(Verse.Map map, GenStepParams parms) {
        if (!AshEra.CanAccumulate(map)) return;
        if (ShouldSkipMap(map)) return;

        // GenStep_ScatterThings.Generate never sets useFallback, so inheriting it would drop the
        // fallback pass. Its stack splitting only matters for items, so run the scatterer's loop.
        //
        // clusterSize has to stay 1. Vanilla's Generate resets clusterCenter and leftInCluster on
        // the way out and this loop cannot - both are private on GenStep_ScatterThings.
        useFallback = false;
        usedSpots.Clear();

        int wanted = CalculateFinalCount(map);
        for (int i = 0; i < wanted; i++) {
            if (!TryFindScatterCell(map, out IntVec3 spot)) {
                if (useFallback || fallbackValidators.NullOrEmpty()) break;

                useFallback = true;
                if (!TryFindScatterCell(map, out spot)) break;
            }

            ScatterAt(spot, map, parms);
            usedSpots.Add(spot);
        }

        usedSpots.Clear();

        // Read back off the lister - ScatterAt can still decline a cell it was handed.
        Logger.Important($"AshVents: scattered {map.listerThings.ThingsOfDef(thingDef).Count} on this map.");
    }

    protected override int CalculateFinalCount(Verse.Map map) {
        return AshVentSiting.CountForExposure(AshmountExposureCache.For(map.Tile));
    }

    protected override bool CanScatterAt(IntVec3 loc, Verse.Map map) {
        if (!base.CanScatterAt(loc, map)) return false;
        if (!loc.Standable(map)) return false;

        bool isWater = loc.GetTerrain(map).IsWater;

        // The base skips validators on the fallback pass, so our own rules relax here too.
        if (useFallback) return AshVentSiting.IsPlausibleFallback(isWater);

        float fertility = map.fertilityGrid.FertilityAt(loc);
        if (!AshVentSiting.PassesCheapRules(fertility, isWater)) return false;

        // Rock quits on its first hit; elevation walks all 317 cells of its disc without one.
        if (!RockNearby(loc, map)) return false;

        return AshVentSiting.IsPlausible(MeanElevation(loc, map), rockNearby: true, fertility, isWater);
    }

    private static float MeanElevation(IntVec3 centre, Verse.Map map) {
        MapGenFloatGrid elevation = MapGenerator.Elevation;
        float total = 0f;
        int counted = 0;

        foreach (IntVec3 cell in GenRadial.RadialCellsAround(centre, ElevationSampleRadius, true)) {
            if (!cell.InBounds(map)) continue;

            total += elevation[cell];
            counted++;
        }

        return counted == 0 ? 0f : total / counted;
    }

    private static bool RockNearby(IntVec3 centre, Verse.Map map) {
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(centre, 6, true)) {
            if (!cell.InBounds(map)) continue;
            if (cell.GetTerrain(map).IsRock) return true;
        }

        return false;
    }
}
