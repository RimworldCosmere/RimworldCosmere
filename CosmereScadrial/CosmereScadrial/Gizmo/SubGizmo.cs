using UnityEngine;
using Verse;

namespace CosmereScadrial.Gizmo;

public abstract class SubGizmo(Verse.Gizmo parent) {
    protected readonly Verse.Gizmo parent = parent;
    public abstract GizmoResult OnGUI(Rect rect);
    public abstract void ProcessInput(Event ev);

    public virtual void OnUpdate(Rect rect) {
        Widgets.DrawHighlight(rect);
    }
}