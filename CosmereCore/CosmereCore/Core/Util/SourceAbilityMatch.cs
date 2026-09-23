namespace Cosmere.Core.Util;

/// <summary>
///     Picking one ability out of a pawn's roster by its def.
/// </summary>
/// <remarks>
///     Kept free of Verse types so it can be tested. Small, but it guards a real failure: a saved
///     hediff used to restore whichever ability sat first on its holder, which on a Mistborn is an
///     arbitrary metal.
/// </remarks>
public static class SourceAbilityMatch {
    /// <summary>Where the wanted def sits in the roster, or -1 if the pawn no longer has it.</summary>
    /// <remarks>
    ///     Missing is an ordinary answer, not an error. An ability can be gone by the time a save
    ///     reloads - spiked out, or the mod that added it removed.
    /// </remarks>
    public static int IndexOf(List<string> abilityDefNames, string? wanted) {
        if (string.IsNullOrEmpty(wanted)) return -1;

        for (int i = 0; i < abilityDefNames.Count; i++) {
            if (abilityDefNames[i] == wanted) return i;
        }

        return -1;
    }
}
