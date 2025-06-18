using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereCore.Utils {
    [StaticConstructorOnStartup]
    public static class MapCosmereGizmoUtility {
        private static Gizmo mouseoverGizmo;
        private static Gizmo lastMouseOverGizmo;
        private static int cacheFrame;
        private static readonly List<object> tmpObjectsList = new List<object>();

        public static Gizmo LastMouseOverGizmo => MapCosmereGizmoUtility.lastMouseOverGizmo;

        public static void MapUIOnGUI()
        {
            if (Find.MainTabsRoot.OpenTab != null && Find.MainTabsRoot.OpenTab != MainButtonDefOf.Inspect)
                return;
            tmpObjectsList.Clear();
            tmpObjectsList.AddRange(Find.Selector.SelectedObjects);
            GizmoGridDrawer.DrawGizmoGridFor(tmpObjectsList, out mouseoverGizmo);
        }

        public static void MapUIUpdate()
        {
            lastMouseOverGizmo = mouseoverGizmo;
            mouseoverGizmo?.GizmoUpdateOnMouseover();
        }
    }
}

