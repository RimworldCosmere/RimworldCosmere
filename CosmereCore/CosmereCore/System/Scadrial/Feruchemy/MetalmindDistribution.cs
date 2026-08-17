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

    /// <summary>Spreads one transfer into the matching sources, and reports how much landed.</summary>
    public static float Fill(
        List<IMetalmindSource> sources,
        string target,
        ConnectionKey? ledgerKey,
        float amount,
        Func<IMetalmindSource, ConnectionKey?, bool> eligible,
        Func<IMetalmindSource, ConnectionKey?, float, float> apply,
        Func<IMetalmindSource, ConnectionKey?, float> room
    ) {
        return Carry(sources, target, ledgerKey, amount, eligible, apply, room, false);
    }

    /// <summary>Draws from the matching sources, never taking more from one than it holds under this key.</summary>
    /// <remarks>
    ///     Every keyed withdrawal goes through here rather than through a per-caller bound, because
    ///     splitting by physical charge let the attribution drain fall through onto another ledger.
    /// </remarks>
    public static float Draw(
        List<IMetalmindSource> sources,
        string target,
        ConnectionKey? ledgerKey,
        float amount,
        Func<IMetalmindSource, ConnectionKey?, bool> eligible,
        Func<IMetalmindSource, ConnectionKey?, float, float> apply,
        Func<IMetalmindSource, ConnectionKey?, float> room
    ) {
        return Carry(sources, target, ledgerKey, amount, eligible, apply, room, true);
    }

    /// <summary>Walks the matching sources handing each what it will take, and reports the total.</summary>
    /// <remarks>
    ///     Dumping the full amount into the first eligible metalmind let its own clamp discard
    ///     the overflow silently, so the remainder is carried to the next one instead.
    /// </remarks>
    private static float Carry(
        List<IMetalmindSource> sources,
        string target,
        ConnectionKey? ledgerKey,
        float amount,
        Func<IMetalmindSource, ConnectionKey?, bool> eligible,
        Func<IMetalmindSource, ConnectionKey?, float, float> apply,
        Func<IMetalmindSource, ConnectionKey?, float> room,
        bool withdrawing
    ) {
        if (amount <= 0f) return 0f;

        float moved = 0f;
        float remaining = amount;

        for (int i = 0; i < sources.Count && remaining > 0f; i++) {
            if (!MatchesTarget(sources[i], target)) continue;
            if (!eligible(sources[i], ledgerKey)) continue;

            float space = room(sources[i], ledgerKey);

            // the bound the whole feature rides on: a keyed draw cannot reach another key's charge.
            if (withdrawing && ledgerKey.HasValue) {
                float attributed = sources[i].StoredFor(ledgerKey.Value);
                if (attributed < space) space = attributed;
            }

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
}
