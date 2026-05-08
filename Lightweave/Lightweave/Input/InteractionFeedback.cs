using Cosmere.Lightweave.Runtime;
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
}
