using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.LesserSpren.SprenController;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Dev;

[StaticConstructorOnStartup]
public static class SprenDebugOverlay {
    public static bool ShowOverlay { get; set; }

    private static Color GetSprenColor(SprenType sprenType) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        return controller?.SprenColor ?? Color.yellow;
    }

    public static void DrawOverlay() {
        if (!ShowOverlay || Find.CurrentMap == null) return;

        Verse.Map map = Find.CurrentMap;
        float zoomLevel = Current.CameraDriver.ZoomRootSize;
        CellRect rect = Current.CameraDriver.CurrentViewRect.ClipInsideMap(map).ExpandedBy(1);
        foreach (BaseSprenController controller in SprenControllerRegistry.EnabledControllers) {
            Color color = GetSprenColor(controller.SprenType);

            if (zoomLevel <= 25) {
                color.a = 0.2f;
                DrawMarkers(controller.ValidSpawnInfo, map, rect, color, MeshPool.plane10, 0f);
            }

            color.a = 0.8f;
            DrawMarkers(controller.ActiveSpawnInfo, map, rect, color, MeshPool.plane05, 0.1f);
        }
    }

    private static void DrawMarkers(
        IReadOnlyCollection<SprenSpawnInformation> infos,
        Verse.Map map,
        CellRect rect,
        Color color,
        Mesh mesh,
        float altitudeOffset
    ) {
        Material material = SolidColorMaterials.SimpleSolidColorMaterial(color);
        foreach (SprenSpawnInformation info in infos) {
            IntVec3 position = info.position!.Value;
            if (!position.InBounds(map) || !rect.Contains(position)) continue;

            Vector3 drawPos = position.ToVector3Shifted();
            drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + altitudeOffset;

            Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
        }
    }
}
