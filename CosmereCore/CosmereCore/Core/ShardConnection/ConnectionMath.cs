// Not Cosmere.Core.Connection: that would shadow the existing type in Cosmere.Core.Comp.Game.
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

    /// <summary>Being born to a world this Shard holds. What ancestry alone is worth.</summary>
    public const int AncestryFloor = 30;

    /// <summary>How tied you must be before a god metal will answer. Ancestry alone clears it.</summary>
    public const int GodMetalThreshold = 30;

    /// <summary>The most that living somewhere can be worth on its own.</summary>
    public const int ResidenceCap = 45;

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
    /// <returns>The Connection to Ruin those spikes are worth, 0 for no spikes, capped so spikes alone cannot pass the ordinary ceiling.</returns>
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

    /// <summary>Composes a Shard tie from one world baseline plus the independent sources.</summary>
    /// <returns>The composed tie: the stronger of ancestry and residence, plus investiture and earned, clamped to 0 through 100.</returns>
    public static int Compose(int ancestry, int residence, int investiture, int earned) {
        int worldTie = ancestry > residence ? ancestry : residence;

        return Clamp(worldTie + investiture + earned);
    }

    /// <summary>Adds the independent sources of Connection to a world.</summary>
    /// <returns>The composed tie to the world, all four sources added together and clamped to 0 through 100.</returns>
    public static int ComposeWorld(int ancestry, int residence, int investiture, int earned) {
        return Clamp(ancestry + residence + investiture + earned);
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
    /// <returns>True when the strength is at or above the god metal threshold.</returns>
    public static bool MayUseGodMetal(int strength) {
        return strength >= GodMetalThreshold;
    }

    /// <summary>
    ///     Harmony holds both Ruin and Preservation, so a Connection to Harmony is a Connection
    ///     to each of them at the same value. Never lowers a strength the pawn already had.
    /// </summary>
    /// <returns>The higher of the pawn's own strength and their Harmony strength.</returns>
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
    /// <returns>The weaker of the Ruin and Preservation strengths.</returns>
    public static int HarmonyFrom(int ruinStrength, int preservationStrength) {
        return ruinStrength < preservationStrength ? ruinStrength : preservationStrength;
    }

    /// <summary>Mirrors Verse.GenDate.TicksPerYear, which the test project cannot reference.</summary>
    public const int TicksPerYear = 3600000;

    /// <summary>Ten years to belong somewhere you were not born.</summary>
    public const int TicksToFullResidence = TicksPerYear * 10;

    /// <summary>
    ///     What living on a world is worth so far. Reaches the residence cap at ten years and
    ///     stops there - naturalising makes you a local, not a native twice over.
    /// </summary>
    /// <returns>The residence points earned so far, 0 for no time lived here and the residence cap at ten years or more.</returns>
    public static int ResidenceFrom(int ticksResident) {
        if (ticksResident <= 0) return 0;
        if (ticksResident >= TicksToFullResidence) return ResidenceCap;

        return (int)((long)ticksResident * ResidenceCap / TicksToFullResidence);
    }

    /// <summary>How long a pawn must have lived here to read at this strength.</summary>
    /// <returns>The ticks of residence that read at this strength, 0 at or below zero strength and the full ten years at or above the residence cap.</returns>
    public static int TicksForResidence(int strength) {
        if (strength <= 0) return 0;
        if (strength >= ResidenceCap) return TicksToFullResidence;

        return (int)((long)strength * TicksToFullResidence / ResidenceCap);
    }

    /// <summary>What a pawn has already banked by existing, which is nothing unless they are local.</summary>
    /// <returns>The residence ticks a pawn starts with, 0 for anyone not native, otherwise their age capped at ten years.</returns>
    public static int SeedTicksForAge(long ageTicks, bool native) {
        if (!native || ageTicks <= 0) return 0;

        return ageTicks >= TicksToFullResidence ? TicksToFullResidence : (int)ageTicks;
    }

    /// <summary>Applies delta to had, bounded by the ceiling's headroom rather than the resulting sum.</summary>
    /// <remarks>
    ///     A had already outside [0, TicksToFullResidence] must not let delta push it further out
    ///     or flip sign - AdjustTicks reports next - had straight back to whoever asked for the move.
    /// </remarks>
    /// <returns>The new tick count, moved by as much of delta as the headroom allows.</returns>
    public static int ClampResidenceTicks(int had, int delta) {
        if (delta > 0) {
            long headroom = global::System.Math.Max(0L, TicksToFullResidence - (long)had);
            return (int)(had + global::System.Math.Min((long)delta, headroom));
        }

        if (delta < 0) {
            long headroom = global::System.Math.Max(0L, (long)had);
            return (int)(had - global::System.Math.Min(-(long)delta, headroom));
        }

        return had;
    }

    /// <summary>How much of a tie can be held elsewhere. Never the last point, so nobody strands themselves.</summary>
    /// <returns>The most Connection that can be held elsewhere, 0 when the total is 1 or less.</returns>
    public static float OffsetCeiling(int composedTotal) {
        return composedTotal <= 1 ? 0f : composedTotal - 1;
    }

    /// <summary>Applies delta to a held offset, bounded by the headroom rather than the resulting sum.</summary>
    /// <remarks>
    ///     The mirror of <see cref="ClampResidenceTicks" />. An offset already past its ceiling - the
    ///     tie shrank after the fact - moves nothing rather than lurching back down to the ceiling.
    /// </remarks>
    /// <returns>The new offset, moved by as much of delta as the headroom allows.</returns>
    public static float ClampOffset(float had, float delta, float ceiling) {
        if (delta > 0f) {
            float headroom = global::System.Math.Max(0f, ceiling - had);
            return had + global::System.Math.Min(delta, headroom);
        }

        if (delta < 0f) {
            float headroom = global::System.Math.Max(0f, had);
            return had - global::System.Math.Min(-delta, headroom);
        }

        return had;
    }

    /// <summary>Converts a stored SpiritWeb edge, which is 0..1, into this scale.</summary>
    /// <returns>The edge value rounded onto this 0 through 100 scale.</returns>
    public static int FromEdge(float edgeValue) {
        return Clamp((int)global::System.Math.Round(edgeValue * Max));
    }

    /// <summary>Converts a strength on this scale back to a SpiritWeb edge.</summary>
    /// <returns>The strength as a 0 to 1 SpiritWeb edge value.</returns>
    public static float ToEdge(int strength) {
        return Clamp(strength) / (float)Max;
    }
}
