using System;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Surface;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Overlay;

internal sealed class DialogWindow : LightweaveWindow
{
    private readonly Func<LightweaveNode> titleBuilder;
    private readonly Func<LightweaveNode> bodyBuilder;
    private readonly Func<LightweaveNode>? footerBuilder;
    private readonly Action onClose;
    private readonly Vector2 size;

    public DialogWindow(
        Func<LightweaveNode> title,
        Func<LightweaveNode> body,
        Func<LightweaveNode>? footer,
        Action onClose,
        Vector2 size)
    {
        this.titleBuilder = title;
        this.bodyBuilder = body;
        this.footerBuilder = footer;
        this.onClose = onClose;
        this.size = size;

        this.doCloseX = true;
        this.doCloseButton = false;
        this.closeOnClickedOutside = false;
        this.draggable = true;
        this.resizeable = false;
    }

    public override Vector2 InitialSize => size;

    protected override LightweaveNode Build()
    {
        LightweaveNode titleNode = titleBuilder();
        LightweaveNode bodyNode = bodyBuilder();
        LightweaveNode? footerNode = footerBuilder?.Invoke();
        return Surface.Surface.Panel(titleNode, bodyNode, footerNode);
    }

    public override void PostClose()
    {
        onClose?.Invoke();
        base.PostClose();
    }
}
