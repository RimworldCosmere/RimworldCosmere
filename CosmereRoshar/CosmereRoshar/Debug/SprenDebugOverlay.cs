using Cosmere.Roshar.LesserSpren.ParticleSystem;
using Cosmere.Roshar.LesserSpren.SprenControllers;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Debug;

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

        // Get all enabled controllers and draw their cells directly
        foreach (BaseSprenController controller in SprenControllerRegistry.GetEnabledControllers()) {
            SprenType sprenType = controller.sprenType;

            // Draw valid spawn cells (semi-transparent)
            Color validColor = GetSprenColor(sprenType);
            validColor.a = 0.2f; // More transparent for valid cells

            foreach (SprenSpawnInformation? info in controller.validSpawnInfo) {
                if (!info.position!.Value.InBounds(Find.CurrentMap)) continue;

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

            // Draw active spawn cells (brighter, smaller)
            Color activeColor = GetSprenColor(sprenType);
            activeColor.a = 0.8f; // Bright for active spawn cells

            foreach (SprenSpawnInformation? info in controller.activeSpawnInfo) {
                if (!info.position!.Value.InBounds(Find.CurrentMap)) continue;

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