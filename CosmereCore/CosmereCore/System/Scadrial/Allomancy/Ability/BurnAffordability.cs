namespace Cosmere.System.Scadrial.Allomancy.Ability;

/// <summary>
///     Whether a burn may change gear. Easing off never costs metal, so only a step up has to be
///     paid for - the rule the ability wheel and the gizmo already enforce, and the dock did not.
/// </summary>
public static class BurnAffordability {
    public static bool Allowed(int currentPower, int nextPower, bool canAfford) {
        return nextPower <= currentPower || canAfford;
    }
}
