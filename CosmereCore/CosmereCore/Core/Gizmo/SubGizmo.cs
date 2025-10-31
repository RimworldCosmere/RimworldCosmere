using UnityEngine;
using Verse;

namespace Cosmere.Core.Gizmo;

public abstract class SubGizmo {
    protected readonly Verse.Gizmo? parent;

    public SubGizmo() { }

    public SubGizmo(Verse.Gizmo parent) {
        this.parent = parent;
    }

    public abstract GizmoResult OnGUI(Rect rect);
    public abstract void ProcessInput(Event ev);

    public virtual void OnUpdate(Rect rect) {
        Widgets.DrawHighlight(rect);
    }
}