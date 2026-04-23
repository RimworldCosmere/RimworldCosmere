using System;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Runtime;

public abstract class LightweaveWindow : Verse.Window
{
    private readonly Guid rootId = Guid.NewGuid();

    protected Guid RootId => rootId;

    protected virtual Theme.Theme? ThemeOverride => null;

    protected abstract LightweaveNode Build();

    public override void DoWindowContents(Rect inRect)
    {
        LightweaveRoot.Render(inRect, rootId, Build, null, ThemeOverride);
    }

    public override void PostClose()
    {
        LightweaveRoot.Release(rootId);
        base.PostClose();
    }
}
