using Cosmere.Core.Settings;
using UnityEngine;

namespace Cosmere.Core.UI.Dock;

// A height that eases toward a target rather than snapping to it. IMGUI has no tween
// engine, so the value is stepped by hand.
//
// Stepping once per frame is the whole point. A dock height is asked for several times
// in a single pass - the density probe, then the window sizing, then the draw - and
// advancing on every one of those would run the animation at three times speed, at a
// rate that changed with how many sections happened to be open.
public sealed class Reveal {
    // A fixed span rather than a fixed speed, so a tall panel and a short one take the
    // same time and the dock feels consistent whichever metal was clicked. Unscaled
    // time, because the dock still has to animate while the game is paused.
    private const float Duration = 0.1f;

    private float from;
    private float goal;
    private float current;
    private float progress = 1f;
    private int steppedFrame = -1;

    public float Toward(float target) {
        if (Mod.GetModSettings<CoreModSettings>().reduceMotion) {
            from = goal = current = target;
            progress = 1f;
            return current;
        }

        // A new target restarts the span from wherever the last one got to, so
        // reversing mid-slide picks up from the current height instead of snapping.
        if (!Mathf.Approximately(target, goal)) {
            from = current;
            goal = target;
            progress = 0f;
        }

        if (steppedFrame == Time.frameCount) return current;

        steppedFrame = Time.frameCount;
        progress = Mathf.Min(1f, progress + (Time.unscaledDeltaTime / Duration));
        current = Mathf.Lerp(from, goal, Smoothstep(progress));

        return current;
    }

    // Leaves and lands without a visible stop at either end.
    private static float Smoothstep(float t) {
        return t * t * (3f - (2f * t));
    }
}
