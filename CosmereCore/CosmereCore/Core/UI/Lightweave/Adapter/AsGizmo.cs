using System;
using Cosmere.Core.UI.Lightweave.Runtime;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Adapter;

/// <summary>
/// Phase 3 adapter that paints a Lightweave tree inside a vanilla Command's gizmo rect.
/// The adapter routes layout/paint through <see cref="LightweaveRoot"/> but does NOT reimplement
/// vanilla Command affordances: disabled + disabledReason handling, hotKey dispatch,
/// right-click FloatMenu opening, TutorSystem gating, UIHighlighter, and Steam Deck activation
/// are all bypassed. Consumers that need those behaviours must either handle them inside
/// their Build() paint, or wait for a later AsGizmo revision that composes with vanilla
/// Command.GizmoOnGUIInt. Subclasses override GetWidth when they need variable width;
/// the base width parameter is a default for fixed-size gizmos.
/// </summary>
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
        if (evt == null || evt.type == EventType.Used)
        {
            return new GizmoResult(GizmoState.Clear);
        }

        bool mouseOver = Mouse.IsOver(rect);
        if (mouseOver && evt.type == EventType.MouseUp && evt.button == 0)
        {
            evt.Use();
            return new GizmoResult(GizmoState.Interacted, evt);
        }

        return new GizmoResult(mouseOver ? GizmoState.Mouseover : GizmoState.Clear);
    }
}
