using Cosmere.Roshar.LesserSpren.ParticleSystem;
using Cosmere.Roshar.LesserSpren.SprenControllers;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Debug;

[StaticConstructorOnStartup]
public static class SprenDebugOverlay {
    private static readonly Material OverlayMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.yellow);

    private static readonly Dictionary<SprenType, Color> SprenColors = new Dictionary<SprenType, Color> {
        [SprenType.Rockspren] = Color.gray,
        [SprenType.Wavespren] = Color.blue,
        [SprenType.Flamespren] = Color.red,
        [SprenType.Grassspren] = Color.green,
        [SprenType.Windspren] = Color.white,
        [SprenType.Sandspren] = Color.yellow,
        [SprenType.Joyspren] = Color.magenta,
        [SprenType.Deathspren] = Color.black,
    };

    public static bool ShowOverlay { get; set; } = false;
    public static bool ShowActualParticles { get; set; } = true;
    public static Dictionary<SprenType, List<IntVec3>> ValidCells { get; } = new Dictionary<SprenType, List<IntVec3>>();

    public static Dictionary<SprenType, List<IntVec3>> ActiveSpawnCells { get; } =
        new Dictionary<SprenType, List<IntVec3>>();

    public static Dictionary<SprenType, List<Vector3>> ActualParticlePositions { get; } =
        new Dictionary<SprenType, List<Vector3>>();

    public static void UpdateValidCells(SprenType sprenType, List<IntVec3> cells) {
        ValidCells[sprenType] = new List<IntVec3>(cells);
    }

    public static void UpdateActiveSpawnCells(SprenType sprenType, List<IntVec3> cells) {
        ActiveSpawnCells[sprenType] = new List<IntVec3>(cells);
    }

    public static void UpdateActualParticles(SprenType sprenType, ParticleSystem particleSystem) {
        if (particleSystem == null || particleSystem.particleCount == 0) {
            ActualParticlePositions[sprenType] = new List<Vector3>();
            return;
        }

        ParticleSystem.Particle[] particles =
            new ParticleSystem.Particle[particleSystem.particleCount];
        int numParticles = particleSystem.GetParticles(particles);

        ActualParticlePositions[sprenType] = particles.Take(numParticles)
            .Select(p => p.position)
            .ToList();
    }

    public static void DrawOverlay() {
        if (!ShowOverlay || Find.CurrentMap == null) return;

        // Draw valid spawn cells (semi-transparent)
        foreach (KeyValuePair<SprenType, List<IntVec3>> kvp in ValidCells) {
            SprenType sprenType = kvp.Key;
            List<IntVec3>? cells = kvp.Value;

            if (!SprenControllerRegistry.IsSprenTypeEnabled(sprenType)) continue;

            Color color = SprenColors.TryGetValue(sprenType, out Color sprenColor) ? sprenColor : Color.yellow;
            color.a = 0.2f; // More transparent for valid cells

            foreach (IntVec3 cell in cells) {
                if (!cell.InBounds(Find.CurrentMap)) continue;

                Vector3 drawPos = cell.ToVector3Shifted();
                drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();

                Graphics.DrawMesh(
                    MeshPool.plane10,
                    drawPos,
                    Quaternion.identity,
                    SolidColorMaterials.SimpleSolidColorMaterial(color),
                    0
                );
            }
        }

        // Draw active spawn cells (brighter, smaller)
        if (ShowActualParticles) {
            foreach (KeyValuePair<SprenType, List<IntVec3>> kvp in ActiveSpawnCells) {
                SprenType sprenType = kvp.Key;
                List<IntVec3>? cells = kvp.Value;

                if (!SprenControllerRegistry.IsSprenTypeEnabled(sprenType)) continue;

                Color color = SprenColors.TryGetValue(sprenType, out Color sprenColor) ? sprenColor : Color.yellow;
                color.a = 0.8f; // Bright for active spawn cells

                foreach (IntVec3 cell in cells) {
                    if (!cell.InBounds(Find.CurrentMap)) continue;

                    Vector3 drawPos = cell.ToVector3Shifted();
                    drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.1f; // Slightly higher

                    Graphics.DrawMesh(
                        MeshPool.plane05, // Smaller mesh for active spawn cells
                        drawPos,
                        Quaternion.identity,
                        SolidColorMaterials.SimpleSolidColorMaterial(color),
                        0
                    );
                }
            }
        }
    }
}