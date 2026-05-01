using Cosmere.Lightweave.Tokens;

namespace Cosmere.Lightweave.Feedback;

public enum BadgeVariant {
    Neutral,
    Accent,
    Warning,
    Danger,
    Success,
}

internal static class BadgeVariants {
    public static ThemeSlot Background(BadgeVariant variant) {
        switch (variant) {
            case BadgeVariant.Accent:
                return ThemeSlot.SurfaceAccent;
            case BadgeVariant.Warning:
                return ThemeSlot.StatusWarning;
            case BadgeVariant.Danger:
                return ThemeSlot.StatusDanger;
            case BadgeVariant.Success:
                return ThemeSlot.StatusSuccess;
            case BadgeVariant.Neutral:
            default:
                return ThemeSlot.SurfaceRaised;
        }
    }

    public static ThemeSlot Foreground(BadgeVariant variant) {
        switch (variant) {
            case BadgeVariant.Accent:
            case BadgeVariant.Warning:
            case BadgeVariant.Danger:
            case BadgeVariant.Success:
                return ThemeSlot.TextOnAccent;
            case BadgeVariant.Neutral:
            default:
                return ThemeSlot.TextPrimary;
        }
    }

    internal static ThemeSlot? Border(BadgeVariant v) {
        switch (v) {
            case BadgeVariant.Neutral:
                return ThemeSlot.BorderDefault;
            case BadgeVariant.Accent:
            case BadgeVariant.Warning:
            case BadgeVariant.Danger:
            case BadgeVariant.Success:
            default:
                return null;
        }
    }
}