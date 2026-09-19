using Cosmere.Core.Util;

namespace Cosmere.System.Roshar.Threat;

/// <summary>
///     What a Radiant is worth to the storyteller, counted in ordinary colonists rather than in
///     points. Vanilla's own colonist is the 1.0 this scale is built on.
/// </summary>
/// <remarks>
///     RimWorld pays 15 points for a colonist at low wealth and 200 at a million, so a flat
///     bonus lands at 2.7 colonists early and a third of one late. A multiple tracks that.
/// </remarks>
public static class RadiantThreat {
    /// <summary>
    ///     A Blade cuts anything it touches and a colonist does not need a bond to swing one, so
    ///     shards are priced on the pawn holding them, not on the bond.
    /// </summary>
    public const float BladeWorth = 1.5f;

    public const float PlateWorth = 2f;

    /// <summary>
    ///     One bond's worth by sworn ideal, First through Fifth. Later ideals unlock abilities
    ///     rather than adding a flat step, so the gaps widen.
    /// </summary>
    private static readonly float[] byIdeal = [1.25f, 1.6f, 2.1f, 2.9f, 3.75f];

    public static int MaxIdeal => byIdeal.Length - 1;

    public static float ForBond(int currentIdeal) {
        int ideal = currentIdeal < 0 ? 0
            : currentIdeal > MaxIdeal ? MaxIdeal
            : currentIdeal;

        return byIdeal[ideal];
    }

    public static float ForShards(bool hasBlade, bool hasPlate) {
        float worth = 0f;
        if (hasBlade) worth += BladeWorth;
        if (hasPlate) worth += PlateWorth;

        return worth;
    }

    /// <summary>
    ///     A whole pawn's worth. A second spren divides a Radiant's attention, so bonds stack
    ///     through DiminishingStack rather than adding up.
    /// </summary>
    public static float ForPawn(IReadOnlyList<float>? bondWorths, float shardWorth) {
        if (bondWorths == null || bondWorths.Count == 0) return 1f + shardWorth;

        List<float> bonds = [];
        for (int i = 0; i < bondWorths.Count; i++) bonds.Add(bondWorths[i]);

        return DiminishingStack.Combine(bonds) + shardWorth;
    }
}
