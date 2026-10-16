using Math = global::System.Math;

namespace Cosmere.System.Scadrial.Threat;

/// <summary>
///     What a metalborn is worth to the storyteller, counted in ordinary colonists rather than in
///     points. Vanilla's own colonist is the 1.0 this scale is built on.
/// </summary>
/// <remarks>
///     The rule this replaced paid a flat 150 for a Mistborn and 20 a metal otherwise, so eight
///     Mistings outbid the Mistborn who beats all of them.
/// </remarks>
public static class ScadrialThreat {
    public const float AllomanticMetalWorth = 0.35f;
    public const float FeruchemicMetalWorth = 0.3f;

    /// <summary>Burning and storing feed each other, so holding both axes is worth more than the sum.</summary>
    public const float TwinbornPremium = 0.25f;

    public const float MistbornWorth = 4f;
    public const float FullFeruchemistWorth = 3f;

    /// <summary>
    ///     A spike already grants its gene through ImplantSpike, so this is the price of the spike
    ///     itself. Billing the granted gene as well charged every spike twice.
    /// </summary>
    public const float SpikeWorth = 0.4f;

    /// <summary>
    ///     A stored attribute is a second body to spend. Metalminds price higher than vials because
    ///     a vial only refills what a Misting already had.
    /// </summary>
    public const float ChargedMetalmindWorth = 0.15f;

    public const float VialWorth = 0.1f;

    /// <summary>A mule's worth of kit is still one pawn, so each axis tops out.</summary>
    public const float MetalmindCeiling = 1f;

    public const float VialCeiling = 0.6f;

    private static float MistbornGain => MistbornWorth - 1f;

    private static float FullFeruchemistGain => FullFeruchemistWorth - 1f;

    /// <summary>
    ///     A metalborn's worth. Single metals never out-price the full gift they are a fraction of,
    ///     which the clamps enforce rather than leaving it to the tuning numbers staying in order.
    /// </summary>
    /// <returns>The pawn's threat worth, counted in ordinary colonists. Never below 1.</returns>
    public static float ForMetalborn(
        bool isMistborn,
        bool isFullFeruchemist,
        int allomanticMetals,
        int feruchemicMetals
    ) {
        int allomantic = allomanticMetals < 0 ? 0 : allomanticMetals;
        int feruchemic = feruchemicMetals < 0 ? 0 : feruchemicMetals;

        float allomancy = isMistborn
            ? MistbornGain
            : Math.Min(allomantic * AllomanticMetalWorth, MistbornGain);
        float feruchemy = isFullFeruchemist
            ? FullFeruchemistGain
            : Math.Min(feruchemic * FeruchemicMetalWorth, FullFeruchemistGain);

        float worth = 1f + allomancy + feruchemy;
        if (allomancy > 0f && feruchemy > 0f) worth += TwinbornPremium;

        return worth;
    }

    public static float ForSpikes(int spikeCount) {
        return spikeCount <= 0 ? 0f : spikeCount * SpikeWorth;
    }

    /// <summary>
    ///     A coppercloud shrinks the whole threat value, not only the Scadrial share of it, but it
    ///     only hides what stands inside it. A cloud burning out in the wilderness conceals nothing.
    /// </summary>
    /// <returns>The threat value with the hidden share taken off. 0 at full coverage.</returns>
    public static float Coppercloud(float threatValue, float coverage) {
        float hidden = Clamp01(coverage);

        return threatValue * (1f - hidden);
    }

    /// <summary>
    ///     What the clouds hide across a whole colony, from the cloud strength over each colonist.
    ///     A colonist standing outside every cloud contributes nothing and still counts as a head.
    /// </summary>
    /// <returns>The mean cloud strength per colonist, 0 to 1. 0 for a null or empty list.</returns>
    public static float Coverage(IReadOnlyList<float>? strengthPerColonist) {
        if (strengthPerColonist == null || strengthPerColonist.Count == 0) return 0f;

        float total = 0f;
        for (int i = 0; i < strengthPerColonist.Count; i++) {
            total += Clamp01(strengthPerColonist[i]);
        }

        return total / strengthPerColonist.Count;
    }

    /// <summary>Local so this file stays free of UnityEngine, which the test host cannot load.</summary>
    private static float Clamp01(float value) {
        if (value < 0f) return 0f;

        return value > 1f ? 1f : value;
    }

    /// <summary>
    ///     What a metalborn's kit is worth. Priced on what the pawn carries rather than on how full
    ///     a reserve is right now, so threat does not flicker every time a vial is drained.
    /// </summary>
    /// <returns>What the carried metalminds and vials add to the pawn's threat worth.</returns>
    public static float ForGear(int chargedMetalminds, int vials) {
        float minds = Math.Min(Math.Max(chargedMetalminds, 0) * ChargedMetalmindWorth, MetalmindCeiling);
        float metal = Math.Min(Math.Max(vials, 0) * VialWorth, VialCeiling);

        return minds + metal;
    }
}
