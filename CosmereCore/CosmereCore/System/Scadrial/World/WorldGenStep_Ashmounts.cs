using System.Collections.Generic;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Comp.Game;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.World;

/// <summary>
///     Raises the Ashmounts: a ring of impassable peaks around a heartland, and the per-tile
///     exposure every map downstream reads.
/// </summary>
public class WorldGenStep_Ashmounts : WorldGenStep {
    /// <summary>Fraction of the layer's tiles that become a mount, before spacing thins it.</summary>
    private const float MountFraction = 0.0016f;

    private const float InnerRadiusTiles = 22f;
    private const float OuterRadiusTiles = 46f;
    private const float MinSpacingTiles = 6f;

    public override int SeedPart => 0x4A5B17;

    /// <summary>
    ///     Deliberately empty. This path runs while a save is deserialising, where the world is not
    ///     yet known and adding world objects would be wrong.
    /// </summary>
    public override void GenerateWithoutWorldData(string seed, PlanetLayer layer) { }

    public override void GenerateFresh(string seed, PlanetLayer layer) {
        // WorldBeforeGenerationPatch sets Primary first, so IsActive's null fallback never fires.
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Ashfall)) {
            AshmountExposureCache.Set(new Dictionary<int, float>());
            return;
        }

        if (!layer.IsRootSurface) {
            AshmountExposureCache.Set(new Dictionary<int, float>());
            return;
        }

        int tileCount = layer.TilesCount;
        if (tileCount <= 0) {
            AshmountExposureCache.Set(new Dictionary<int, float>());
            return;
        }

        int centreId = Rand.Range(0, tileCount);
        PlanetTile centre = new PlanetTile(centreId, layer);

        List<AshmountRing.Candidate> candidates = new List<AshmountRing.Candidate>();
        WorldGrid grid = Find.WorldGrid;

        // at order 350 nothing upstream makes a MapParent; kept against a third-party step running earlier.
        HashSet<int> taken = new HashSet<int>();
        foreach (MapParent parent in Find.WorldObjects.MapParents) {
            if (parent.Tile.Layer == layer) taken.Add(parent.Tile.tileId);
        }

        for (int i = 0; i < tileCount; i++) {
            PlanetTile tile = new PlanetTile(i, layer);
            if (taken.Contains(i)) continue;

            Tile data = grid[tile];
            if (data.PrimaryBiome == null || !data.PrimaryBiome.canBuildBase) continue;

            float distance = layer.ApproxDistanceInTiles(centre, tile);
            if (distance > OuterRadiusTiles) continue;

            bool hilly = data.hilliness == Hilliness.LargeHills || data.hilliness == Hilliness.Mountainous;
            candidates.Add(new AshmountRing.Candidate(i, distance, hilly));
        }

        int wanted = Mathf.Max(6, Mathf.RoundToInt(tileCount * MountFraction));
        List<int> mounts = AshmountRing.Choose(
            candidates,
            InnerRadiusTiles,
            OuterRadiusTiles,
            wanted,
            MinSpacingTiles,
            (a, b) => layer.ApproxDistanceInTiles(a, b),
            Gen.HashCombineInt(GenText.StableStringHash(seed), SeedPart)
        );

        for (int i = 0; i < mounts.Count; i++) {
            PlanetTile tile = new PlanetTile(mounts[i], layer);

            // Impassable is the settle block: TileFinder.IsValidTileForNewSettlement rejects it outright.
            grid[tile].hilliness = Hilliness.Impassable;

            WorldObject mount = WorldObjectMaker.MakeWorldObject(AshmountDefOf.Cosmere_Scadrial_WorldObject_Ashmount);
            mount.Tile = tile;
            Find.WorldObjects.Add(mount);
        }

        CacheExposure(layer, mounts, tileCount);
        Log.Info($"Ashmounts: raised {mounts.Count} around tile {centreId}.");
    }

    private static void CacheExposure(PlanetLayer layer, List<int> mounts, int tileCount) {
        Dictionary<int, float> exposure = new Dictionary<int, float>();
        List<float> distances = new List<float>();

        for (int i = 0; i < tileCount; i++) {
            distances.Clear();

            for (int m = 0; m < mounts.Count; m++) {
                float d = layer.ApproxDistanceInTiles(i, mounts[m]);
                if (d < AshmountExposure.RangeTiles) distances.Add(d);
            }

            if (distances.Count == 0) continue;

            exposure[i] = AshmountExposure.Multiplier(distances);
        }

        AshmountExposureCache.Set(exposure);
    }
}
