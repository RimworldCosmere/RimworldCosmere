using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Lightweave.Input;

internal static class InteractionFeedback {
    public static void Apply(Rect rect, bool enabled, bool playSound) {
        if (!enabled) {
            if (Mouse.IsOver(rect)) {
                CursorOverrides.MarkDisabledHover();
            }

            return;
        }

        if (playSound) {
            MouseoverSounds.DoRegion(rect);
        }
    }

    public static Color OverlayColor(Theme.Theme theme, InteractionState state, float alpha) {
        return OverlayColor(theme, state.Pressed, alpha);
    }

    public static Color OverlayColor(Theme.Theme theme, bool pressed, float alpha) {
        ThemeSlot slot = pressed ? ThemeSlot.InteractionPress : ThemeSlot.InteractionHover;
        Color baseColor = theme.GetColor(slot);
        return new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }
}
