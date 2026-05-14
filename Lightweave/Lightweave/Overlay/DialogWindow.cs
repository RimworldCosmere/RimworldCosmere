using System;
using Cosmere.Lightweave.Runtime;
using UnityEngine;

namespace Cosmere.Lightweave.Overlay;

internal sealed class DialogWindow : LightweaveWindow {
    private readonly Func<LightweaveNode> contentBuilder;
    private readonly Action onDismiss;
    private readonly DialogShellOptions options;

    public DialogWindow(
        Func<LightweaveNode> content,
        Action onDismiss,
        DialogShellOptions options
    ) {
        contentBuilder = content;
        this.onDismiss = onDismiss;
        this.options = options;

        doCloseX = false;
        doCloseButton = false;
        closeOnClickedOutside = false;
        draggable = false;
        resizeable = false;
        forcePause = true;
        absorbInputAroundWindow = options.IsModal;
        closeOnAccept = false;
        closeOnCancel = true;
        drawShadow = false;
        doWindowBackground = false;
        layer = Verse.WindowLayer.Super;
    }

    protected override bool DrawBorder => false;

    protected override bool EdgeResizable => false;

    public override Vector2 InitialSize => new Vector2(Verse.UI.screenWidth, Verse.UI.screenHeight);

    protected override float Margin => 0f;

    protected override LightweaveNode Body() {
        return Dialog.BuildShell(contentBuilder(), options);
    }

    public override void PostClose() {
        onDismiss?.Invoke();
        base.PostClose();
    }
}