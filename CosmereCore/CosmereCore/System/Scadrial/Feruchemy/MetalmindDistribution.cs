using System;

namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     Which metalminds a transfer reaches, and how one transfer is spread across them.
/// </summary>
/// <remarks>
///     Deliberately free of RimWorld and Unity types so the test host can load it.
/// </remarks>
public static class MetalmindDistribution {
    // Every metalmind, which is what a pawn carrying a dozen wants by default.
    public const string TargetAll = "";

    // Worn and carried metalminds only. Safe to draw on, since none of them can
    // be compounded and so none of them can be burned away.
    public const string TargetExternal = "group:external";

    // Implanted metalminds only, which is everything compounding can reach.
    public const string TargetInternal = "group:internal";

    public static bool IsGroupTarget(string target) {
        return target is TargetAll or TargetExternal or TargetInternal;
    }

    // A target naming a metalmind that has since burned out matches nothing, which keeps
    // charge from landing somewhere the player did not pick.
    public static bool MatchesTarget(IMetalmindSource source, string target) {
        return target switch {
            TargetAll => true,
            TargetInternal => source.IsImplanted,
            TargetExternal => !source.IsImplanted,
            _ => source.SourceId == target,
        };
    }

    /// <summary>Spreads one transfer across the matching sources, and reports how much moved.</summary>
    /// <remarks>
    ///     Dumping the full amount into the first eligible metalmind let its own clamp discard
    ///     the overflow silently, so the remainder is carried to the next one instead.
    /// </remarks>
    public static float Carry(
        List<IMetalmindSource> sources,
        string target,
        float amount,
        Func<IMetalmindSource, bool> eligible,
        Func<IMetalmindSource, float, float> apply,
        Func<IMetalmindSource, float> room
    ) {
        if (amount <= 0f) return 0f;

        float moved = 0f;
        float remaining = amount;

        for (int i = 0; i < sources.Count && remaining > 0f; i++) {
            if (!MatchesTarget(sources[i], target)) continue;
            if (!eligible(sources[i])) continue;

            float space = room(sources[i]);
            float take = space < remaining ? space : remaining;
            if (take <= 0f) continue;

            // What it took, not what it was offered. A metalmind can look able to take a
            // transfer and refuse it - Metalmind.AddStored bails on ValidateOwner - and counting
            // the offer paid the pawn for charge that never landed.
            float took = apply(sources[i], take);
            if (took <= 0f) continue;

            remaining -= took;
            moved += took;
        }

        return moved;
    }
}
