using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

public static class ScadrialMetallurgyConstants {
    /**
     * How much of a metalmind's base capacity survives the smithing. A well-made
     * metalmind holds more of an attribute than a crude one, enough that quality
     * outweighs the choice of metalmind: a legendary earring carries more than an
     * awful bracelet.
     */
    public static float MetalmindCapacityFactor(QualityCategory quality) {
        return quality switch {
            QualityCategory.Awful => 0.5f,
            QualityCategory.Poor => 0.75f,
            QualityCategory.Normal => 1f,
            QualityCategory.Good => 1.25f,
            QualityCategory.Excellent => 1.5f,
            QualityCategory.Masterwork => 1.75f,
            QualityCategory.Legendary => 2f,
            _ => 1f,
        };
    }

    /**
     * Based on a FULL mistborn burning every metal being capped around 5 BEUs
     * 5 BEU / 16 metals / 100 units per metal ends up with this conversion rate
     */
    public const float BreathEquivalentUnitsPerMetalUnit = 0.3125f;

    public const float VialMetalAmount = 1f;
    public const float RawMetalMetalAmount = 1f;

    public const float AllomancyXPPerTick = 1 / (float)GenTicks.TickRareInterval;
    public const float FeruchemyXPPerTick = 1 / (float)GenTicks.TickRareInterval;
}
