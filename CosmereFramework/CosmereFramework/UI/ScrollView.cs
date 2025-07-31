using System;
using UnityEngine;
using Verse;

namespace Cosmere.Framework.UI;

public class ScrollViewStatus {
    public float Height;
    public Vector2 Position;
}

public readonly record struct ScrollView : IDisposable {
    private readonly float outRectHeight;

    public readonly Rect rect;
    private readonly ScrollViewStatus scrollViewStatus;

    public ScrollView(Rect outRect, ScrollViewStatus scrollViewStatus, bool showScrollbars = true) {
        this.scrollViewStatus = scrollViewStatus;
        outRectHeight = outRect.height;
        rect = new Rect(0f, 0f, outRect.width, Math.Max(height, outRectHeight));
        if (height - 0.1f >= outRect.height) {
            rect.width -= 20f;
        }

        height = 0f;
        Widgets.BeginScrollView(outRect, ref this.scrollViewStatus.Position, rect, showScrollbars);
    }

    public ref float height => ref scrollViewStatus.Height;

    public void Dispose() {
        Widgets.EndScrollView();
    }

    public bool CanCull(float entryHeight, float entryY) {
        return entryY + entryHeight < scrollViewStatus.Position.y ||
               entryY > scrollViewStatus.Position.y + outRectHeight;
    }
}