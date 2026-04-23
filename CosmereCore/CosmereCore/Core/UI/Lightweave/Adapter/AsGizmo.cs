using System;
using Cosmere.Core.UI.Lightweave.Runtime;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Adapter;

public abstract class AsGizmo : Command
{
    private readonly int entityId;
    private readonly AdapterKind adapterKind;
    protected readonly float width;
    protected readonly float height;

    protected AsGizmo(int entityId, float width = 75f, float height = 75f, AdapterKind adapterKind = AdapterKind.Gizmo)
    {
        this.entityId = entityId;
        this.adapterKind = adapterKind;
        this.width = width;
        this.height = height;
    }

    public override float GetWidth(float maxWidth) => Mathf.Min(width, maxWidth);

    protected abstract LightweaveNode Build();

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), height);
        Guid id = AdapterStoreRegistry.Get(entityId, adapterKind);
        LightweaveRoot.Render(rect, id, Build);
        return new GizmoResult(Mouse.IsOver(rect) ? GizmoState.Mouseover : GizmoState.Clear);
    }
}
