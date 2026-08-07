using Cosmere.System.Scadrial.Comp.Game;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.MapGen;

/// <summary>
///     Scatters ash vents across a map, more of them the closer the tile sits to an Ashmount.
/// </summary>
public class GenStep_ScatterAshVents : GenStep_Scatterer {
    private const int ElevationSampleRadius = 10;

    private int placed;

    public override int SeedPart => 0x5AE17;

    /// <summary>
    ///     Gate lives here, not in the XPath patch. A patched-away genStep list breaks games that
    ///     never wanted the mod's content - the failure shape from 18518fe7.
    /// </summary>
    public override void Generate(Verse.Map map, GenStepParams parms) {
        if (!AshEra.CanAccumulate(map)) return;

        count = AshVentSiting.CountForExposure(AshmountExposureCache.For(map.Tile));
        placed = 0;
        base.Generate(map, parms);

        // Counted in ScatterAt, not from usedSpots - Generate clears that list before it returns.
        Logger.Important($"AshVents: scattered {placed} on this map.");
    }

    protected override bool CanScatterAt(IntVec3 loc, Verse.Map map) {
        if (!base.CanScatterAt(loc, map)) return false;
        if (!loc.Standable(map)) return false;

        bool isWater = loc.GetTerrain(map).IsWater;

        // Vanilla only runs the fallback pass when fallbackValidators is non-empty, which is the
        // whole reason the def carries one.
        if (useFallback) return AshVentSiting.IsPlausibleFallback(isWater);

        float fertility = map.fertilityGrid.FertilityAt(loc);
        if (!AshVentSiting.PassesCheapRules(fertility, isWater)) return false;

        // Last on purpose. IsPlausible takes its arguments eagerly, and these two discs are ~430
        // grid reads against 1000 candidates a vent.
        return AshVentSiting.IsPlausible(MeanElevation(loc, map), RockNearby(loc, map), fertility, isWater);
    }

    protected override void ScatterAt(IntVec3 loc, Verse.Map map, GenStepParams parms, int stackCount = 1) {
        GenSpawn.Spawn(ThingDefOf_AshVent.Cosmere_Scadrial_Thing_AshVent, loc, map);
        placed++;
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

[DefOf]
public static class ThingDefOf_AshVent {
    public static ThingDef Cosmere_Scadrial_Thing_AshVent = null!;

    static ThingDefOf_AshVent() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_AshVent));
    }
}
