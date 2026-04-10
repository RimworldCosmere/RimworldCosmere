using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.LesserSpren.SprenController;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Debug;

[StaticConstructorOnStartup]
public static class SprenDebugOverlay {
    public static bool showOverlay { get; set; }

    // Get color from controller instead of hardcoded dictionary
    private static Color GetSprenColor(SprenType sprenType) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        return controller?.sprenColor ?? Color.yellow; // Default fallback color
    }


    public static void DrawOverlay() {
        if (!showOverlay || Find.CurrentMap == null) return;

        float zoomLevel = Current.CameraDriver.ZoomRootSize;
        CellRect rect = Current.CameraDriver.CurrentViewRect.ClipInsideMap(Find.CurrentMap).ExpandedBy(1);
        // Get all enabled controllers and draw their cells directly
        foreach (BaseSprenController controller in SprenControllerRegistry.enabledControllers) {
            SprenType sprenType = controller.sprenType;

            if (zoomLevel <= 25) {
                // Draw valid spawn cells (semi-transparent)
                Color validColor = GetSprenColor(sprenType);
                validColor.a = 0.2f; // More transparent for valid cells
                foreach (SprenSpawnInformation? info in controller.validSpawnInfo) {
                    IntVec3 position = info.position!.Value;
                    if (!position.InBounds(Find.CurrentMap) || !rect.Contains(position)) continue;

                    Vector3 drawPos = info.position!.Value.ToVector3Shifted();
                    drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();

                    Graphics.DrawMesh(
                        MeshPool.plane10,
                        drawPos,
                        Quaternion.identity,
                        SolidColorMaterials.SimpleSolidColorMaterial(validColor),
                        0
                    );
                }
            }

            // Draw active spawn cells (brighter, smaller)
            Color activeColor = GetSprenColor(sprenType);
            activeColor.a = 0.8f; // Bright for active spawn cells

            foreach (SprenSpawnInformation? info in controller.activeSpawnInfo) {
                IntVec3 position = info.position!.Value;
                if (!position.InBounds(Find.CurrentMap) || !rect.Contains(position)) continue;

                Vector3 drawPos = info.position!.Value.ToVector3Shifted();
                drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.1f; // Slightly higher

                Graphics.DrawMesh(
                    MeshPool.plane05, // Smaller mesh for active spawn cells
                    drawPos,
                    Quaternion.identity,
                    SolidColorMaterials.SimpleSolidColorMaterial(activeColor),
                    0
                );
            }
        }
    }
}