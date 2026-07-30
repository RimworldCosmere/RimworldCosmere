using System;
using System.Collections.Generic;

namespace Cosmere.Core.Quest;

/// <summary>
///     Verse-free weighted selection over a reward table. Tables must sum to exactly 100 so
///     a def author reading the XML sees percentages rather than opaque relative weights.
/// </summary>
public static class RewardTable {
    public static int TotalWeight(List<RewardTableEntry> entries) {
        if (entries == null) return 0;

        int total = 0;
        for (int i = 0; i < entries.Count; i++) {
            total += entries[i].weight;
        }

        return total;
    }

    public static bool IsValid(List<RewardTableEntry> entries) {
        if (entries == null || entries.Count == 0) return false;

        for (int i = 0; i < entries.Count; i++) {
            if (entries[i].weight <= 0) return false;
            if (string.IsNullOrEmpty(entries[i].key)) return false;
        }

        return TotalWeight(entries) == 100;
    }

    public static string? Roll(List<RewardTableEntry> entries, int seed) {
        if (entries == null || entries.Count == 0) return null;

        int total = TotalWeight(entries);
        if (total <= 0) return null;

        int pick = new Random(seed).Next(total);
        int running = 0;
        for (int i = 0; i < entries.Count; i++) {
            running += entries[i].weight;
            if (pick < running) return entries[i].key;
        }

        return entries[entries.Count - 1].key;
    }
}
