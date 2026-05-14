using Cosmere.Lightweave.Tokens;

namespace Cosmere.Lightweave.Input;

public enum ButtonVariant {
    Primary,
    Secondary,
    Ghost,
    Danger,
    Frosted,
}

internal static class ButtonVariants {
    public static ThemeSlot? Background(ButtonVariant variant, InteractionState state, bool ghost = false) {
        if (state.Disabled) {
            return ghost ? (ThemeSlot?)null : ThemeSlot.SurfaceDisabled;
        }

        if (ghost) {
            return null;
        }

        switch (variant) {
            case ButtonVariant.Primary:
                return ThemeSlot.SurfaceAccent;
            case ButtonVariant.Secondary:
                if (state.Pressed) {
                    return ThemeSlot.SurfaceSunken;
                }

                if (state.Hovered) {
                    return ThemeSlot.SurfaceRaised;
                }

                return ThemeSlot.SurfaceTranslucent;
            case ButtonVariant.Ghost:
                return null;
            case ButtonVariant.Danger:
                return ThemeSlot.StatusDanger;
            case ButtonVariant.Frosted:
                return ThemeSlot.SurfacePrimary;
            default:
                return ThemeSlot.SurfacePrimary;
        }
    }

    public static ThemeSlot Foreground(ButtonVariant variant, InteractionState state, bool ghost = false) {
        if (state.Disabled) {
            return ThemeSlot.TextMuted;
        }

        if (ghost) {
            switch (variant) {
                case ButtonVariant.Primary:
                    return ThemeSlot.SurfaceAccent;
                case ButtonVariant.Danger:
                    return ThemeSlot.StatusDanger;
                default:
                    return ThemeSlot.TextPrimary;
            }
        }

        switch (variant) {
            case ButtonVariant.Primary:
                return ThemeSlot.TextOnAccent;
            case ButtonVariant.Danger:
                return ThemeSlot.TextOnDanger;
            case ButtonVariant.Secondary:
            case ButtonVariant.Ghost:
                return ThemeSlot.TextPrimary;
            case ButtonVariant.Frosted:
                return state.Hovered || state.Pressed
                    ? ThemeSlot.TextOnAccent
                    : ThemeSlot.TextPrimary;
            default:
                return ThemeSlot.TextPrimary;
        }
    }

    public static ThemeSlot? Border(ButtonVariant variant, InteractionState state, bool ghost = false) {
        if (state.Disabled) {
            return ghost ? null : ThemeSlot.BorderSubtle;
        }

        if (ghost) {
            switch (variant) {
                case ButtonVariant.Primary:
                    return ThemeSlot.SurfaceAccent;
                case ButtonVariant.Danger:
                    return ThemeSlot.StatusDanger;
                default:
                    return null;
            }
        }

        switch (variant) {
            case ButtonVariant.Primary:
            case ButtonVariant.Danger:
                return ThemeSlot.BorderDefault;
            case ButtonVariant.Secondary:
                return state.Hovered ? ThemeSlot.BorderHover : ThemeSlot.BorderDefault;
            case ButtonVariant.Ghost:
                return state.Hovered ? ThemeSlot.BorderHover : (ThemeSlot?)null;
            case ButtonVariant.Frosted:
                return state.Hovered || state.Pressed ? ThemeSlot.BorderHover : ThemeSlot.BorderSubtle;
            default:
                return ThemeSlot.BorderDefault;
        }
    }

    public static float OverlayAlpha(InteractionState state) {
        if (state.Disabled) {
            return 0f;
        }

        if (state.Pressed) {
            return 0.22f;
        }

        if (state.Hovered) {
            return 0.14f;
        }

        return 0f;
    }
}