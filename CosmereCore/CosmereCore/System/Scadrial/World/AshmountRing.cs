using System;
using System.Collections.Generic;

namespace Cosmere.System.Scadrial.World;

/// <summary>
///     Which tiles get a mount. Pure and Verse-free: the geometry is a world-generation decision
///     the player can never undo, so it is worth being able to test without a planet.
/// </summary>
public static class AshmountRing {
    /// <summary>A tile the ring may use, with the two facts the choice turns on.</summary>
    public readonly struct Candidate {
        public readonly int TileId;
        public readonly float DistanceFromCentre;
        public readonly bool Hilly;

        public Candidate(int tileId, float distanceFromCentre, bool hilly) {
            TileId = tileId;
            DistanceFromCentre = distanceFromCentre;
            Hilly = hilly;
        }
    }

    /// <summary>
    ///     Greedy: shuffle within the annulus, hills first, and accept whatever still clears the
    ///     spacing. Returns fewer than asked for rather than looping when the band is too tight.
    /// </summary>
    public static List<int> Choose(
        IReadOnlyList<Candidate> candidates,
        float innerRadius,
        float outerRadius,
        int wanted,
        float minSpacing,
        Func<int, int, float> distanceBetween,
        int seed
    ) {
        List<int> chosen = new List<int>();
        if (candidates.Count == 0 || wanted <= 0) return chosen;

        List<Candidate> pool = new List<Candidate>();
        for (int i = 0; i < candidates.Count; i++) {
            float d = candidates[i].DistanceFromCentre;
            if (d < innerRadius || d > outerRadius) continue;

            pool.Add(candidates[i]);
        }

        // Hills first, then a seeded hash so the order is stable for a seed without being biased
        // toward low tile ids.
        pool.Sort((a, b) => {
            if (a.Hilly != b.Hilly) return a.Hilly ? -1 : 1;
            return Shuffle(a.TileId, seed).CompareTo(Shuffle(b.TileId, seed));
        });

        for (int i = 0; i < pool.Count && chosen.Count < wanted; i++) {
            int tile = pool[i].TileId;

            bool crowded = false;
            for (int j = 0; j < chosen.Count; j++) {
                if (distanceBetween(tile, chosen[j]) < minSpacing) {
                    crowded = true;
                    break;
                }
            }

            if (!crowded) chosen.Add(tile);
        }

        return chosen;
    }

    private static uint Shuffle(int tileId, int seed) {
        uint hash = (uint)tileId * 2654435761u;
        hash ^= (uint)seed * 2246822519u;
        hash ^= hash >> 15;
        hash *= 2246822519u;
        hash ^= hash >> 13;
        return hash;
    }
}
