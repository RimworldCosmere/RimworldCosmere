using System;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI;

public class ScrollViewStatus {
    public float height;
    public Vector2 position;
    public bool scrollVisibile;
}

public readonly record struct ScrollView : IDisposable {
    private readonly float outRectHeight;

    public readonly Rect rect;
    private readonly ScrollViewStatus scrollViewStatus;

    public ScrollView(
        Rect outRect,
        ScrollViewStatus scrollViewStatus,
        bool showScrollbars = true
    ) {
        this.scrollViewStatus = scrollViewStatus;
        outRectHeight = outRect.height;
        rect = new Rect(0, 0, outRect.width, Math.Max(Height, outRectHeight));
        this.scrollViewStatus.scrollVisibile = Height - 0.1f >= outRect.height;
        if (this.scrollViewStatus.scrollVisibile) {
            rect.width -= 20f;
        }

        Height = 0f;
        Widgets.BeginScrollView(outRect, ref this.scrollViewStatus.position, rect, showScrollbars);
    }

    public ref float Height => ref scrollViewStatus.height;

    public void Dispose() {
        Widgets.EndScrollView();
    }

    public bool CanCull(float entryHeight, float entryY) {
        return entryY + entryHeight < scrollViewStatus.position.y ||
               entryY > scrollViewStatus.position.y + outRectHeight;
    }
}
