namespace Cosmere.System.Scadrial.Allomancy;

/// <summary>
///     Decides which end of a steelpush or ironpull is the one that actually moves.
/// </summary>
public static class AllomanticShove {
    // Buildings declare no Mass statBase, so GetStatValue hands back the StatDef default of 1kg and
    // a wall reads lighter than the Allomancer shoving off it. 24f over the caster's own mass puts
    // the pawn roughly eight tiles out at baseline strength.
    public const float AnchorMassSurplus = 24f;

    public static float EffectiveTargetMass(float measuredMass, float casterMass, bool anchored) {
        return anchored ? casterMass + AnchorMassSurplus : measuredMass;
    }

    public static bool MovesCaster(float targetMass, float casterMass) {
        return targetMass > casterMass;
    }
}
