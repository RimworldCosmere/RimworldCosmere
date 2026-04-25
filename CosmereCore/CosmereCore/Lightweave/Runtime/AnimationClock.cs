using System;

namespace Cosmere.Lightweave.Runtime;

/// <summary>
///     Tracks which roots have in-flight animations for the current frame.
///     LightweaveWindow hosts repaint automatically every frame, so no forced
///     repaint is needed there. One-shot Render callers are responsible for
///     calling Render again if HasActiveForFrame returns true.
/// </summary>
public static class AnimationClock {
    private static readonly HashSet<Guid> activeThisFrame = new HashSet<Guid>();

    public static void RegisterActive(Guid rootId) {
        activeThisFrame.Add(rootId);
    }

    public static bool HasActiveForFrame(Guid rootId) {
        return activeThisFrame.Contains(rootId);
    }

    public static void ClearFrame() {
        activeThisFrame.Clear();
    }
}