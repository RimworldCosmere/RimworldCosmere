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

    /// <summary>Walks the matching sources handing each what it will take, and reports the total.</summary>
    /// <remarks>
    ///     Dumping the full amount into the first eligible metalmind let its own clamp discard
    ///     the overflow silently, so the remainder is carried to the next one instead.
    /// </remarks>
    public static float Transfer(
        List<IMetalmindSource> sources,
        MetalmindOperation operation,
        string target,
        ConnectionKey? ledgerKey,
        float amount
    ) {
        if (amount <= 0f) return 0f;

        float moved = 0f;
        float remaining = amount;

        for (int i = 0; i < sources.Count && remaining > 0f; i++) {
            IMetalmindSource source = sources[i];
            if (!MatchesTarget(source, target)) continue;
            if (!IsEligible(source, operation)) continue;

            float space = Room(source, operation);

            // the bound the whole feature rides on: a keyed withdrawal cannot reach another key's charge.
            if (!IsFill(operation) && ledgerKey.HasValue) {
                float attributed = source.StoredFor(ledgerKey.Value);
                if (attributed < space) space = attributed;
            }

            float take = space < remaining ? space : remaining;
            if (take <= 0f) continue;

            // use what it took, not the offer: AddStored can refuse via ValidateOwner and take less.
            float took = Apply(source, operation, ledgerKey, take);
            if (took <= 0f) continue;

            remaining -= took;
            moved += took;
        }

        return moved;
    }

    /// Only the stores get an unbounded walk. An operation nobody added here is bounded by the
    /// key, so a forgotten entry under-fills where it can be seen instead of minting Connection.
    private static bool IsFill(MetalmindOperation operation) {
        return operation is MetalmindOperation.Store or MetalmindOperation.StoreCompounded;
    }

    private static bool IsEligible(IMetalmindSource source, MetalmindOperation operation) {
        return operation switch {
            MetalmindOperation.Store => source.CanStore,
            MetalmindOperation.StoreCompounded => source.CanStoreCompounded,
            MetalmindOperation.Tap => source.CanTap,
            MetalmindOperation.TapCompounded => source.CanTapCompounded,
            _ => false,
        };
    }

    private static float Room(IMetalmindSource source, MetalmindOperation operation) {
        return operation switch {
            MetalmindOperation.Store => source.FreeSpace,
            MetalmindOperation.StoreCompounded => source.FreeSpace,
            MetalmindOperation.Tap => source.StoredAmount,

            // burning draws on the whole charge; reading only the compounded pool stalled the burn.
            MetalmindOperation.TapCompounded => source.TotalStored,
            _ => 0f,
        };
    }

    private static float Apply(
        IMetalmindSource source,
        MetalmindOperation operation,
        ConnectionKey? ledgerKey,
        float amount
    ) {
        return operation switch {
            MetalmindOperation.Store => source.AddStored(amount, ledgerKey),
            MetalmindOperation.StoreCompounded => source.AddCompounded(amount),
            MetalmindOperation.Tap => source.ConsumeStored(amount, ledgerKey),
            MetalmindOperation.TapCompounded => source.ConsumeCompounded(amount, ledgerKey),
            _ => 0f,
        };
    }
}
