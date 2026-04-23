using System;
using Cosmere.Core.UI.Lightweave.Runtime;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Adapter;

public abstract class AsGizmo : Command
{
    private readonly int entityId;
    private readonly int subKey;
    private readonly AdapterKind adapterKind;
    protected readonly float width;
    protected readonly float height;

    protected AsGizmo(int entityId, int subKey = 0, float width = 75f, float height = 75f, AdapterKind adapterKind = AdapterKind.Gizmo)
    {
        this.entityId = entityId;
        this.subKey = subKey;
        this.adapterKind = adapterKind;
        this.width = width;
        this.height = height;
    }

    public override float GetWidth(float maxWidth) => Mathf.Min(width, maxWidth);

    protected abstract LightweaveNode Build();

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), height);
        Guid id = AdapterStoreRegistry.Get(entityId, adapterKind, subKey);
        LightweaveRoot.Render(rect, id, Build);

        Event evt = Event.current;
        bool mouseOver = Mouse.IsOver(rect);
        if (mouseOver && evt.type == EventType.MouseUp && evt.button == 0)
        {
            return new GizmoResult(GizmoState.Interacted, evt);
        }

        return new GizmoResult(mouseOver ? GizmoState.Mouseover : GizmoState.Clear);
    }
}
