namespace Cosmere.Core.BetaHub;

/// <summary>
///     Where the two feedback plates sit. Returns floats rather than Rects so the arithmetic
///     stays testable outside Unity.
/// </summary>
public static class FeedbackButtonLayout {
    public const float ButtonWidth = 200f;
    public const float ButtonHeight = 30f;
    public const float EdgeMargin = 8f;
    public const float ButtonGap = 4f;
    public const float TopMargin = 8f;

    public static float ButtonX(float screenWidth, bool learningHelperVisible) {
        float x = screenWidth - EdgeMargin - ButtonWidth;
        if (learningHelperVisible) x -= EdgeMargin + ButtonWidth;

        return x;
    }

    public static float ButtonY(int index) {
        return TopMargin + index * (ButtonHeight + ButtonGap);
    }
}
