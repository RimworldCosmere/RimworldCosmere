using System;
using Cosmere.Lightweave.Runtime;
using UnityEngine;

namespace Cosmere.Lightweave.Overlay;

internal sealed class DialogWindow : LightweaveWindow {
    private readonly Func<LightweaveNode> contentBuilder;
    private readonly Action onClose;
    private readonly Vector2 size;

    public DialogWindow(
        Func<LightweaveNode> content,
        Action onClose,
        Vector2 size
    ) {
        contentBuilder = content;
        this.onClose = onClose;
        this.size = size;

        doCloseX = true;
        doCloseButton = false;
        closeOnClickedOutside = false;
        draggable = true;
        resizeable = false;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnAccept = false;
        closeOnCancel = true;
        drawShadow = true;
    }

    protected override bool DrawBorder => false;

    public override Vector2 InitialSize {
        get {
            if (size.y > 0f) {
                return size;
            }

            LightweaveNode probe = contentBuilder();
            float width = size.x > 0f ? size.x : 480f;
            float height = probe.Measure?.Invoke(width) ?? probe.PreferredHeight ?? 240f;
            return new Vector2(width, height + 8f);
        }
    }

    protected override LightweaveNode Build() {
        return contentBuilder();
    }

    public override void PostClose() {
        onClose?.Invoke();
        base.PostClose();
    }
}