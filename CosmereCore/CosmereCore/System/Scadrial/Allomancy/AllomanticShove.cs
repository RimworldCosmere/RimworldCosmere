namespace Cosmere.System.Scadrial.Allomancy;

/// <summary>
///     Decides which end of a steelpush or ironpull is the one that actually moves.
/// </summary>
public static class AllomanticShove {
    /// <summary>
    ///     Buildings declare no Mass statBase, so GetStatValue defaults to 1kg and a wall reads
    ///     lighter than the Allomancer shoving off it. 24f over caster mass puts the pawn ~8 tiles out.
    /// </summary>
    public const float AnchorMassSurplus = 24f;

    public static float EffectiveTargetMass(float measuredMass, float casterMass, bool anchored) {
        return anchored ? casterMass + AnchorMassSurplus : measuredMass;
    }

    public static bool MovesCaster(float targetMass, float casterMass) {
        return targetMass > casterMass;
    }
}
