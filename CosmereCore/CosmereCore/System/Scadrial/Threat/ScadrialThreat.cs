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
            : Min(allomantic * AllomanticMetalWorth, MistbornGain);
        float feruchemy = isFullFeruchemist
            ? FullFeruchemistGain
            : Min(feruchemic * FeruchemicMetalWorth, FullFeruchemistGain);

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
    public static float Coppercloud(float threatValue, float coverage) {
        float hidden = coverage < 0f ? 0f : coverage > 1f ? 1f : coverage;

        return threatValue * (1f - hidden);
    }

    /// <summary>
    ///     What the clouds hide across a whole colony, from the cloud strength over each colonist.
    ///     A colonist standing outside every cloud contributes nothing and still counts as a head.
    /// </summary>
    public static float Coverage(IReadOnlyList<float>? strengthPerColonist) {
        if (strengthPerColonist == null || strengthPerColonist.Count == 0) return 0f;

        float total = 0f;
        for (int i = 0; i < strengthPerColonist.Count; i++) {
            float strength = strengthPerColonist[i];
            total += strength < 0f ? 0f : strength > 1f ? 1f : strength;
        }

        return total / strengthPerColonist.Count;
    }

    /// <summary>
    ///     What a metalborn's kit is worth. Priced on what the pawn carries rather than on how full
    ///     a reserve is right now, so threat does not flicker every time a vial is drained.
    /// </summary>
    public static float ForGear(int chargedMetalminds, int vials) {
        float minds = Min(Max(chargedMetalminds, 0) * ChargedMetalmindWorth, MetalmindCeiling);
        float metal = Min(Max(vials, 0) * VialWorth, VialCeiling);

        return minds + metal;
    }

    private static int Max(int a, int b) => a > b ? a : b;

    private static float Min(float a, float b) => a < b ? a : b;
}
