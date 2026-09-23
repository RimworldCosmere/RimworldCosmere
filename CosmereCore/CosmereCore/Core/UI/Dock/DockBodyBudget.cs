namespace Cosmere.Core.UI.Dock;

/// <summary>
///     How much room a dock section's body gets after the pinned strip has taken its share.
/// </summary>
public static class DockBodyBudget {
    /// <summary>
    ///     Never negative. A tall pinned strip or a bottomed-out window used to hand the scroll view
    ///     a negative rect, which drew the next header over the strip above it.
    /// </summary>
    public static float For(float remaining, float sectionMax, float pinned) {
        float byScreen = remaining > 0f ? remaining : 0f;
        float bySetting = sectionMax - pinned;
        if (bySetting < 0f) bySetting = 0f;

        return byScreen < bySetting ? byScreen : bySetting;
    }
}
