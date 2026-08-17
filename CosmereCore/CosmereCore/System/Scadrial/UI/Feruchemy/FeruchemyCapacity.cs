using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;

namespace Cosmere.System.Scadrial.UI.Feruchemy;

internal readonly struct FeruchemyCapacity {
    public FeruchemyCapacity(
        float internalHeld,
        float externalHeld,
        float internalMax,
        float externalMax,
        float max,
        bool hasMetalmind,
        bool canTap,
        bool canStore,
        bool canTapCompounded,
        bool canStoreCompounded,
        float compoundedRate
    ) {
        Internal = internalHeld;
        External = externalHeld;
        InternalMax = internalMax;
        ExternalMax = externalMax;
        Max = max;
        HasMetalmind = hasMetalmind;
        CanTap = canTap;
        CanStore = canStore;
        CanTapCompounded = canTapCompounded;
        CanStoreCompounded = canStoreCompounded;
        CompoundedRate = compoundedRate;
    }

    // Charge held in implanted metalminds.
    public float Internal { get; }

    // Charge held in worn and carried metalminds.
    public float External { get; }

    public float InternalMax { get; }

    public float ExternalMax { get; }

    // Each kind of metalmind reads against its own capacity, and a kind the
    // pawn has none of is left out rather than shown as a flat zero.
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

    public float Max { get; }

    public bool HasMetalmind { get; }

    public bool CanTap { get; }

    public bool CanStore { get; }

    public bool CanTapCompounded { get; }

    public bool CanStoreCompounded { get; }

    // Positive while filling the pool, negative while burning the metalmind.
    public float CompoundedRate { get; }

    public float Fraction => Max > 0f ? (Internal + External) / Max : 0f;

    // Each band fills against its own capacity, so a full set of implants reads
    // as a full bar rather than as its share of the combined total. That keeps
    // the bars saying the same thing as the percentages above them.
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

        return new FeruchemyCapacity(
            internalHeld,
            externalHeld,
            internalMax,
            externalMax,
            max,
            sources.Count > 0,
            gene.canTap,
            gene.canStore,
            gene.canTapCompounded,
            gene.canStoreCompounded,
            gene.CompoundedRatePerSecond
        );
    }
}
