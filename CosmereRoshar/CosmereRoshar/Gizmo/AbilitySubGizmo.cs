using Cosmere.Core.Gizmo;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Gizmo;

[StaticConstructorOnStartup]
public class AbilitySubGizmo : SubGizmo {
    public override GizmoResult OnGUI(Rect rect) {
        return new GizmoResult(GizmoState.Clear, null);
    }

    public override void ProcessInput(Event ev) { }
}