using System;

namespace Cosmere.System.Roshar.Dialog;

/// <summary>
///     Rect arithmetic for the order filmstrip: the sigil rail along the footer and the two
///     edge arrow columns.
/// </summary>
/// <remarks>
///     Kept free of Unity types so the geometry can be tested on a plain runtime. Every value
///     is derived from a passed-in rect edge, never from an assumed window coordinate.
/// </remarks>
public static class RadiantFilmstripLayout {
    public const float DotSize = 34f;
    public const float DotGap = 10f;
    public const float SelectedScale = 1.3f;
    public const float SelectedRingWidth = 3f;
    public const float ArrowColumnWidth = 72f;

    public static float SelectedDotSize => DotSize * SelectedScale;

    /// <summary>Width of the whole dot group, gaps included.</summary>
    public static float RailWidth(int count) {
        if (count <= 0) return 0f;

        return count * DotSize + (count - 1) * DotGap;
    }

    /// <summary>Height the rail needs so the enlarged dot and its ring still fit.</summary>
    public static float RailHeight() {
        return SelectedDotSize + SelectedRingWidth * 2f;
    }

    public static float RailLeft(float centerX, int count) {
        return centerX - RailWidth(count) / 2f;
    }

    public static float DotCenterX(float centerX, int count, int index) {
        return RailLeft(centerX, count) + index * (DotSize + DotGap) + DotSize / 2f;
    }

    public static float DotSizeAt(int index, int selectedIndex) {
        return index == selectedIndex ? SelectedDotSize : DotSize;
    }

    /// <summary>Arrow columns stop at the rail so they never cover a dot or the join button.</summary>
    public static float ArrowColumnHeight(float contentTop, float railTop) {
        return Math.Max(0f, railTop - contentTop);
    }

    public static float RightArrowLeft(float contentLeft, float contentWidth) {
        return contentLeft + contentWidth - ArrowColumnWidth;
    }
}
