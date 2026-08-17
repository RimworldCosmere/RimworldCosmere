using System;

namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     Pure map arithmetic behind a metalmind's charge-by-ledger attribution: total, rescale to a
///     new total, and proportional drain. Verse-free.
/// </summary>
public static class ChargeAttribution {
    /// <summary>Sums the map. Null or empty totals 0.</summary>
    public static float Total(Dictionary<string, float>? map) {
        if (map == null) return 0f;

        float total = 0f;
        foreach (KeyValuePair<string, float> pair in map) total += pair.Value;

        return total;
    }

    /// <summary>Scales every entry so the map sums to <paramref name="target" />. Empties the map when the target, or the map's own total, is at or below zero.</summary>
    public static void Rescale(Dictionary<string, float> map, float target) {
        float total = Total(map);
        target = Math.Max(0f, target);
        if (total <= 0f || target <= 0f) {
            map.Clear();
            return;
        }

        float factor = target / total;
        List<string> keys = new List<string>(map.Keys);
        for (int i = 0; i < keys.Count; i++) map[keys[i]] = Math.Max(0f, map[keys[i]] * factor);
    }

    /// <summary>Removes up to <paramref name="amount" />, split by each entry's share of the total. Returns what was actually removed.</summary>
    public static float Drain(Dictionary<string, float> map, float amount) {
        float total = Total(map);
        amount = Math.Max(0f, amount);
        if (total <= 0f || amount <= 0f) return 0f;

        if (amount >= total) {
            map.Clear();
            return total;
        }

        float factor = amount / total;
        List<string> keys = new List<string>(map.Keys);
        for (int i = 0; i < keys.Count; i++) map[keys[i]] = Math.Max(0f, map[keys[i]] * (1f - factor));

        return amount;
    }
}
