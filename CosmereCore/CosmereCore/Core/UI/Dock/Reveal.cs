using Cosmere.Core.Settings;
using UnityEngine;

namespace Cosmere.Core.UI.Dock;

/// <summary>
///     Height that eases toward a target, stepped by hand once per frame since IMGUI has no tween engine.
///     Multiple calls in one pass (probe, sizing, draw) must not each step, or the animation runs too fast.
/// </summary>
public sealed class Reveal {
    /// <summary>
    ///     Fixed span, not fixed speed, so a tall panel and a short one take the same time. Unscaled
    ///     time, because the dock must still animate while the game is paused.
    /// </summary>
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

        // new target restarts the span from the current height, so reversing mid-slide does not snap.
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
