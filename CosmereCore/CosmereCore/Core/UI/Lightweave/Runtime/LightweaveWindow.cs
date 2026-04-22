using System;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Runtime;

public abstract class LightweaveWindow : Verse.Window
{
    private readonly Guid rootId = Guid.NewGuid();

    protected abstract LightweaveNode Build();

    public override void DoWindowContents(Rect inRect)
    {
        LightweaveRoot.Render(inRect, rootId, Build);
    }

    public override void PostClose()
    {
        LightweaveRoot.Release(rootId);
        base.PostClose();
    }
}
