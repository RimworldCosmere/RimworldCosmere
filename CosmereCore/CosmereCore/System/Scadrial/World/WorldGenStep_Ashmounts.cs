using System.Collections.Generic;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Comp.Game;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

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

    public override void GenerateFresh(string seed, PlanetLayer layer) {
        // WorldBeforeGenerationPatch guarantees Primary is set before GenerateWorld runs, so
        // this cannot see the null-permissive fallback inside IsActive.
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Ashfall)) return;
        if (!layer.IsRootSurface) return;

        int tileCount = layer.TilesCount;
        if (tileCount <= 0) return;

        int centreId = Rand.Range(0, tileCount);
        PlanetTile centre = new PlanetTile(centreId, layer);

        List<AshmountRing.Candidate> candidates = new List<AshmountRing.Candidate>();
        WorldGrid grid = Find.WorldGrid;

        // Belt and braces with the order-450 slot: nothing should own a tile this early, but a
        // mod placing objects sooner must not have one stamped impassable underneath it.
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
            Gen.HashCombineInt(seed.GetHashCode(), SeedPart)
        );

        WorldObjectDef def = DefDatabase<WorldObjectDef>.GetNamed("Cosmere_Scadrial_WorldObject_Ashmount");

        for (int i = 0; i < mounts.Count; i++) {
            PlanetTile tile = new PlanetTile(mounts[i], layer);

            // Impassable is the settle block. TileFinder.IsValidTileForNewSettlement rejects it
            // outright, so this needs no Harmony patch and it renders as a range for free.
            grid[tile].hilliness = Hilliness.Impassable;

            WorldObject mount = WorldObjectMaker.MakeWorldObject(def);
            mount.Tile = tile;
            Find.WorldObjects.Add(mount);
        }

        CacheExposure(layer, mounts, tileCount);
        Logger.Important($"Ashmounts: raised {mounts.Count} around tile {centreId}.");
    }

    private static void CacheExposure(PlanetLayer layer, List<int> mounts, int tileCount) {
        Dictionary<int, float> exposure = new Dictionary<int, float>();
        List<float> distances = new List<float>();

        for (int i = 0; i < tileCount; i++) {
            PlanetTile tile = new PlanetTile(i, layer);
            distances.Clear();

            for (int m = 0; m < mounts.Count; m++) {
                float d = layer.ApproxDistanceInTiles(tile, new PlanetTile(mounts[m], layer));
                if (d < AshmountExposure.RangeTiles) distances.Add(d);
            }

            if (distances.Count == 0) continue;

            exposure[i] = AshmountExposure.Multiplier(distances);
        }

        AshmountExposureCache.Set(exposure);
    }
}
