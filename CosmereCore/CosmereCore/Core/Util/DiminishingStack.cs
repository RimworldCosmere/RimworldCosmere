namespace Cosmere.Core.Util;

/// <summary>
///     What several sources of the same effect are worth together.
/// </summary>
/// <remarks>
///     Kept free of Verse types so it can be tested.
/// </remarks>
public static class DiminishingStack {
    /// <summary>What each further source is worth against the one before it.</summary>
    private const float Falloff = 0.5f;

    /// <summary>
    ///     Adds sources up with each one counting for half of the last.
    /// </summary>
    /// <remarks>
    ///     Two beat one and four never beat two twice over, so stacking buys something real without
    ///     letting a colony brick an effect by rostering enough bodies. Sorted strongest first -
    ///     otherwise the answer would depend on the order the map happened to hand the sources over,
    ///     and the same colony would measure differently after a save reload.
    ///     <para>Sorts the list it is given.</para>
    /// </remarks>
    public static float Combine(List<float> strengths) {
        strengths.Sort();

        float total = 0f;
        float share = 1f;
        for (int i = strengths.Count - 1; i >= 0; i--) {
            total += strengths[i] * share;
            share *= Falloff;
        }

        return total;
    }
}
