using System;

namespace Cosmere.Lightweave.Runtime.Internal;

internal sealed class OverlayQueue {
    private readonly List<Action> pending = new List<Action>();

    public void Enqueue(Action drawOverlay) {
        pending.Add(drawOverlay);
    }

    public void Flush() {
        for (int i = 0; i < pending.Count; i++) {
            pending[i]();
        }

        pending.Clear();
    }

    public void Clear() {
        pending.Clear();
    }
}