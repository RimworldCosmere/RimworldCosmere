using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereCore.Utils;

[StaticConstructorOnStartup]
public static class MapCosmereGizmoUtility {
    private static Gizmo MouseoverGizmo;
    private static int CacheFrame;
    private static readonly List<object> TMPObjectsList = new List<object>();

    public static Gizmo lastMouseOverGizmo { get; private set; }

    public static void MapUIOnGUI() {
        if (Find.MainTabsRoot.OpenTab != null && Find.MainTabsRoot.OpenTab != MainButtonDefOf.Inspect) {
            return;
        }

        TMPObjectsList.Clear();
        TMPObjectsList.AddRange(Find.Selector.SelectedObjects);
        GizmoGridDrawer.DrawGizmoGridFor(TMPObjectsList, out MouseoverGizmo);
    }

    public static void MapUIUpdate() {
        lastMouseOverGizmo = MouseoverGizmo;
        MouseoverGizmo?.GizmoUpdateOnMouseover();
    }
}