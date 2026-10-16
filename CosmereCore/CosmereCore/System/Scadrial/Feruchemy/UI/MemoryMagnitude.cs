namespace Cosmere.System.Scadrial.Feruchemy.UI;

/// <summary>
///     How a memory's mood weight is written and ordered in the store-memory window.
/// </summary>
/// <remarks>
///     Kept clear of Verse and Unity so the ordering can be tested. Colour alone cannot carry
///     the sign - red and green are the pair most colour-blind players cannot separate, and
///     choosing what to offload is the whole job of that window.
/// </remarks>
public static class MemoryMagnitude {
    /// <summary>
    ///     A mood offset as a signed, fixed-width figure.
    /// </summary>
    /// <returns>The offset written to one decimal place with an explicit sign, so the column stays aligned.</returns>
    public static string Format(float offset) {
        return offset.ToString("+0.0;-0.0;0.0");
    }

    /// <summary>
    ///     Orders by weight, heaviest first, whichever way the memory pulls.
    /// </summary>
    /// <returns>A negative number when a weighs more, a positive one when b does, and 0 when they weigh the same.</returns>
    public static int CompareByWeight(float a, float b) {
        return global::System.Math.Abs(b).CompareTo(global::System.Math.Abs(a));
    }
}
