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

    /// Worn and carried metalminds only. Safe to draw on, since none of them can
    /// be compounded and so none of them can be burned away.
    public const string TargetExternal = "group:external";

    // Implanted metalminds only, which is everything compounding can reach.
    public const string TargetInternal = "group:internal";

    public static bool IsGroupTarget(string target) {
        return target is TargetAll or TargetExternal or TargetInternal;
    }

    /// A target naming a metalmind that has since burned out matches nothing, which keeps
    /// charge from landing somewhere the player did not pick.
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
        string? ledgerKey,
        float amount,
        Func<IMetalmindSource, string?, bool> eligible,
        Func<IMetalmindSource, string?, float, float> apply,
        Func<IMetalmindSource, string?, float> room
    ) {
        if (amount <= 0f) return 0f;

        float moved = 0f;
        float remaining = amount;

        for (int i = 0; i < sources.Count && remaining > 0f; i++) {
            if (!MatchesTarget(sources[i], target)) continue;
            if (!eligible(sources[i], ledgerKey)) continue;

            float space = room(sources[i], ledgerKey);
            float take = space < remaining ? space : remaining;
            if (take <= 0f) continue;

            // use what it took, not the offer: AddStored can refuse via ValidateOwner and take less.
            float took = apply(sources[i], ledgerKey, take);
            if (took <= 0f) continue;

            remaining -= took;
            moved += took;
        }

        return moved;
    }

    /// <summary>Takes charge back out of what one key is recorded as holding, never out of another key's.</summary>
    /// <remarks>
    ///     Bounding the ask by the key's own balance is what stops a correction falling through to a
    ///     second ledger, which would mint one tie by destroying another.
    /// </remarks>
    public static float Reclaim(List<IMetalmindSource> sources, string target, string ledgerKey, float amount) {
        return Carry(
            sources,
            target,
            ledgerKey,
            amount,
            static (m, k) => k != null && m.CanTap && m.StoredFor(k) > 0f,
            static (m, k, a) => m.ConsumeStored(a, k),
            static (m, k) => k == null ? 0f : m.StoredFor(k)
        );
    }
}
