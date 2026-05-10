using Cosmere.Lightweave.Tokens;

namespace Cosmere.Lightweave.Input;

public enum ButtonVariant {
    Primary,
    Secondary,
    Ghost,
    Danger,
    Dock,
}

internal static class ButtonVariants {
    public static ThemeSlot Background(ButtonVariant variant, InteractionState state) {
        if (state.Disabled) {
            return ThemeSlot.SurfaceDisabled;
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

                return ThemeSlot.SurfacePrimary;
            case ButtonVariant.Ghost:
                if (state.Pressed) {
                    return ThemeSlot.SurfaceSunken;
                }

                if (state.Hovered) {
                    return ThemeSlot.SurfaceRaised;
                }

                return ThemeSlot.SurfacePrimary;
            case ButtonVariant.Danger:
                return ThemeSlot.StatusDanger;
            case ButtonVariant.Dock:
                return ThemeSlot.SurfacePrimary;
            default:
                return ThemeSlot.SurfacePrimary;
        }
    }

    public static ThemeSlot Foreground(ButtonVariant variant, InteractionState state) {
        if (state.Disabled) {
            return ThemeSlot.TextMuted;
        }

        switch (variant) {
            case ButtonVariant.Primary:
                return ThemeSlot.TextOnAccent;
            case ButtonVariant.Danger:
                return ThemeSlot.TextOnDanger;
            case ButtonVariant.Secondary:
            case ButtonVariant.Ghost:
                return ThemeSlot.TextPrimary;
            case ButtonVariant.Dock:
                return state.Hovered || state.Pressed
                    ? ThemeSlot.TextOnAccent
                    : ThemeSlot.TextPrimary;
            default:
                return ThemeSlot.TextPrimary;
        }
    }

    public static ThemeSlot? Border(ButtonVariant variant, InteractionState state) {
        if (state.Disabled) {
            return ThemeSlot.BorderSubtle;
        }

        switch (variant) {
            case ButtonVariant.Primary:
            case ButtonVariant.Danger:
                return ThemeSlot.BorderDefault;
            case ButtonVariant.Secondary:
                return state.Hovered ? ThemeSlot.BorderHover : ThemeSlot.BorderDefault;
            case ButtonVariant.Ghost:
                return state.Hovered ? ThemeSlot.BorderSubtle : null;
            case ButtonVariant.Dock:
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