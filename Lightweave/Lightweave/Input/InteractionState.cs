using Cosmere.Lightweave.Runtime;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Input;

internal readonly record struct InteractionState(bool Hovered, bool Pressed, bool Focused, bool Disabled) {
    public static InteractionState Resolve(Rect rect, string? focusName, bool disabled) {
        LightweaveHitTracker.Track(rect);
        if (disabled) {
            if (Mouse.IsOver(rect)) {
                CursorOverrides.MarkDisabledHover();
            }

            return new InteractionState(false, false, false, true);
        }

        bool hovered = Mouse.IsOver(rect);
        bool pressed = hovered && UnityEngine.Input.GetMouseButton(0);
        bool focused = focusName != null && GUI.GetNameOfFocusedControl() == focusName;
        return new InteractionState(hovered, pressed, focused, false);
    }
}