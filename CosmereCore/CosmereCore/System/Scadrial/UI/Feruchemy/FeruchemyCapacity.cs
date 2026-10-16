using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;

namespace Cosmere.System.Scadrial.UI.Feruchemy;

internal readonly struct FeruchemyCapacity {
    // Charge held in implanted metalminds.
    public float Internal { get; init; }

    // Charge held in worn and carried metalminds.
    public float External { get; init; }

    public float InternalMax { get; init; }

    public float ExternalMax { get; init; }

    /// <summary>
    ///     Gets the capacity line shown to the player. Each kind of metalmind reads against its own
    ///     capacity, and a kind the pawn has none of is left out rather than shown as a flat zero.
    /// </summary>
    public string Readout {
        get {
            bool hasInternal = InternalMax > 0f;
            bool hasExternal = ExternalMax > 0f;

            if (hasInternal && hasExternal) {
                return $"{Internal / InternalMax * 100f:0}% + {External / ExternalMax * 100f:0}%";
            }

            if (hasInternal) return $"{Internal / InternalMax * 100f:0}%";
            if (hasExternal) return $"{External / ExternalMax * 100f:0}%";

            return "0%";
        }
    }

    // The same split in raw units, for the hover.
    public string UnitsReadout {
        get {
            bool hasInternal = InternalMax > 0f;
            bool hasExternal = ExternalMax > 0f;

            if (hasInternal && hasExternal) {
                return $"{Internal:0}/{InternalMax:0} implanted, {External:0}/{ExternalMax:0} worn";
            }

            if (hasInternal) return $"{Internal:0}/{InternalMax:0} implanted";
            if (hasExternal) return $"{External:0}/{ExternalMax:0} worn";

            return "0";
        }
    }

    public float Max { get; init; }

    public bool HasMetalmind { get; init; }

    public bool CanTap { get; init; }

    public bool CanStore { get; init; }

    public bool CanTapCompounded { get; init; }

    public bool CanStoreCompounded { get; init; }

    // Positive while filling the pool, negative while burning the metalmind.
    public float CompoundedRate { get; init; }

    public float Fraction => Max > 0f ? (Internal + External) / Max : 0f;

    /// <summary>
    ///     Gets how full the worn metalminds are. Each band fills against its own capacity, so a full set
    ///     reads as a full bar rather than its share of the combined total, matching the percentages above.
    /// </summary>
    public float StoredFraction => ExternalMax > 0f ? External / ExternalMax : 0f;

    public float CompoundedFraction => InternalMax > 0f ? Internal / InternalMax : 0f;

    public static FeruchemyCapacity Of(Feruchemist? gene) {
        if (gene == null) return default;

        List<IMetalmindSource> sources = gene.metalminds;
        float internalHeld = 0f;
        float externalHeld = 0f;
        float internalMax = 0f;
        float externalMax = 0f;
        float max = 0f;
        for (int i = 0; i < sources.Count; i++) {
            if (sources[i].IsImplanted) {
                internalHeld += sources[i].TotalStored;
                internalMax += sources[i].MaxAmount;
            } else {
                externalHeld += sources[i].TotalStored;
                externalMax += sources[i].MaxAmount;
            }

            max += sources[i].MaxAmount;
        }

        return new FeruchemyCapacity {
            Internal = internalHeld,
            External = externalHeld,
            InternalMax = internalMax,
            ExternalMax = externalMax,
            Max = max,
            HasMetalmind = sources.Count > 0,
            CanTap = gene.canTap,
            CanStore = gene.canStore,
            CanTapCompounded = gene.canTapCompounded,
            CanStoreCompounded = gene.canStoreCompounded,
            CompoundedRate = gene.CompoundedRatePerSecond,
        };
    }
}
