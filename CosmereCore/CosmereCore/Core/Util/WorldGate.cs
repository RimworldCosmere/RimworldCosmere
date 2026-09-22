namespace Cosmere.Core.Util;

/// <summary>Whether content belonging to one world may run on a save set to another.</summary>
/// <remarks>Pure on purpose: everything around it is Verse-bound and cannot be loaded off-game.</remarks>
public static class WorldGate {
    /// <summary>
    ///     Permissive in every unknown case. Only two known worlds that disagree fail.
    /// </summary>
    public static bool Matches(string? contentWorld, string? saveWorld, bool crossWorld) {
        if (string.IsNullOrEmpty(contentWorld)) return true;
        if (crossWorld) return true;
        if (string.IsNullOrEmpty(saveWorld)) return true;

        return contentWorld == saveWorld;
    }
}
