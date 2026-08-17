// Not Cosmere.Core.Connection: that shadows the existing Connection type in
// Cosmere.Core.Comp.Game, and every unrelated file using it stops compiling.
namespace Cosmere.Core.ShardConnection;

/// <summary>
///     The arithmetic behind a pawn's Connection to a Shard.
/// </summary>
/// <remarks>
///     Verse-free on purpose. Every number here decides who may burn a god metal and who may
///     gain a power at all, so it is worth being able to test directly rather than through a
///     running game.
///     <para>
///         Storage lives in the SpiritWeb, which holds the earned portion as a 0..1 edge between
///         the pawn and the Shard. Ancestry and Investiture are recomputed rather than stored, so
///         removing a gene drops the total on its own with no migration step.
///     </para>
/// </remarks>
public static class ConnectionMath {
    public const int Max = 100;

    /// <summary>What being born to the Shard's world is worth on its own.</summary>
    public const int AncestryFloor = 30;

    /// <summary>The threshold to use a god metal, or to gain a power at all.</summary>
    public const int TouchedThreshold = 1;

    public const int BondedThreshold = 31;
    public const int InvestedThreshold = 71;

    /// <summary>A Misting or a Ferring.</summary>
    public const int SingleInvestitureBonus = 10;

    /// <summary>A Mistborn or a Full Feruchemist.</summary>
    public const int FullInvestitureBonus = 20;

    /// <summary>What one hemalurgic spike is worth toward Ruin.</summary>
    public const int SpikeBonus = 10;

    /// <summary>
    ///     The ceiling for anyone who is not holding a Shard.
    /// </summary>
    /// <remarks>
    ///     Ascendant is reserved for a Shardholder. A kandra wearing all four Blessings carries
    ///     eight spikes, which would otherwise walk straight through the top of the scale.
    /// </remarks>
    public const int OrdinaryMax = 99;

    /// <summary>
    ///     What a body full of Ruin's metal is worth on its own.
    /// </summary>
    /// <remarks>
    ///     Hemalurgy is Ruin's alone, so this never reads against Preservation - and must not,
    ///     because Harmony is derived as the lower of the two and would rise with it.
    /// </remarks>
    public static int StrengthFromSpikes(int spikes) {
        if (spikes <= 0) return 0;

        return global::System.Math.Min(spikes * SpikeBonus, OrdinaryMax - AncestryFloor);
    }

    public static ConnectionTier TierOf(int strength) {
        if (strength >= Max) return ConnectionTier.Ascendant;
        if (strength >= InvestedThreshold) return ConnectionTier.Invested;
        if (strength >= BondedThreshold) return ConnectionTier.Bonded;
        if (strength >= TouchedThreshold) return ConnectionTier.Touched;

        return ConnectionTier.None;
    }

    /// <summary>
    ///     Composes one pawn's strength toward one Shard.
    /// </summary>
    /// <remarks>
    ///     Ancestry and residence are two routes to the same base rather than two additions:
    ///     naturalising on a world you were not born to eventually reaches what being born there
    ///     grants, and does not exceed it. Investiture and earned strength stack on top of that.
    /// </remarks>
    public static int Compose(int ancestry, int residence, int investiture, int earned) {
        int baseline = ancestry > residence ? ancestry : residence;

        return Clamp(baseline + investiture + earned);
    }

    public static int Clamp(int strength) {
        if (strength < 0) return 0;
        return strength > Max ? Max : strength;
    }

    /// <summary>
    ///     Whether this strength is enough to burn a god metal or gain a new power.
    /// </summary>
    /// <remarks>
    ///     Lerasium is the deliberate exception and is not asked about here - burning it is how
    ///     an unconnected person becomes Connected in the first place, so gating it on Connection
    ///     would make it useless to the only people it is for.
    /// </remarks>
    public static bool MayUseGodMetal(int strength) {
        return strength >= AncestryFloor;
    }

    /// <summary>
    ///     Harmony holds both Ruin and Preservation, so a Connection to Harmony is a Connection
    ///     to each of them at the same value. Never lowers a strength the pawn already had.
    /// </summary>
    public static int WithHarmony(int ownStrength, int harmonyStrength) {
        return ownStrength > harmonyStrength ? ownStrength : harmonyStrength;
    }

    /// <summary>
    ///     Harmony is Ruin and Preservation held together, so being tied to both is being tied to
    ///     Harmony - at the weaker of the two, since half of Harmony is not Harmony.
    /// </summary>
    /// <remarks>
    ///     The mirror of <see cref="WithHarmony" />, so the two directions cannot disagree.
    ///     Nothing grants a floor to Harmony directly - ancestry hands out floors from a world's
    ///     fallback Shards, and Scadrial's are Ruin and Preservation - so without this a
    ///     post-Catacendre native reads 0 to their own world's Shard.
    ///     <para>
    ///         Not about harmonium. Harmonium reacts with water and would kill anyone who
    ///         swallowed it, which is why it carries no allomancy or feruchemy at all and never
    ///         reaches the god metal gate.
    ///     </para>
    /// </remarks>
    public static int HarmonyFrom(int ruinStrength, int preservationStrength) {
        return ruinStrength < preservationStrength ? ruinStrength : preservationStrength;
    }

    /// <summary>Mirrors Verse.GenDate.TicksPerYear, which the test project cannot reference.</summary>
    public const int TicksPerYear = 3600000;

    /// <summary>
    ///     What living on a world is worth so far. Reaches the ancestry floor at one year and
    ///     stops there - naturalising makes you a local, not a native twice over.
    /// </summary>
    public static int ResidenceFrom(int ticksResident) {
        if (ticksResident <= 0) return 0;
        if (ticksResident >= TicksPerYear) return AncestryFloor;

        return (int)((long)ticksResident * AncestryFloor / TicksPerYear);
    }

    /// <summary>How long a pawn must have lived here to read at this strength.</summary>
    public static int TicksForResidence(int strength) {
        if (strength <= 0) return 0;
        if (strength >= AncestryFloor) return TicksPerYear;

        return (int)((long)strength * TicksPerYear / AncestryFloor);
    }

    /// <summary>Adds delta to had and clamps to [0, TicksPerYear] without overflowing on a large delta.</summary>
    public static int ClampResidenceTicks(int had, int delta) {
        long next = (long)had + delta;
        if (next < 0) return 0;

        return next > TicksPerYear ? TicksPerYear : (int)next;
    }

    /// <summary>Converts a stored SpiritWeb edge, which is 0..1, into this scale.</summary>
    public static int FromEdge(float edgeValue) {
        return Clamp((int)global::System.Math.Round(edgeValue * Max));
    }

    /// <summary>Converts a strength on this scale back to a SpiritWeb edge.</summary>
    public static float ToEdge(int strength) {
        return Clamp(strength) / (float)Max;
    }
}
