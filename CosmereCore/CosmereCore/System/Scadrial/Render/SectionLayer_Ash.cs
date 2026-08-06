using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Grid;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Render;

/// <summary>
///     Settled ash, drawn the way RimWorld draws its own terrain smears: scattered decals at
///     random size and rotation rather than a flat wash over every cell. Density, size and
///     opacity all ride the depth underneath, so the ground thickens instead of just darkening.
/// </summary>
[StaticConstructorOnStartup]
public class SectionLayer_Ash : SectionLayer {
    private const float MinSize = 0.9f;
    private const float MaxSize = 2.6f;
    private const float MaxOpacity = 0.8f;

    private static readonly Material[] SmearMats = [
        MaterialPool.MatFrom("Terrain/Scatter/AshSmearA", Verse.ShaderDatabase.Transparent),
        MaterialPool.MatFrom("Terrain/Scatter/AshSmearB", Verse.ShaderDatabase.Transparent),
    ];

    private static readonly Color32 Shallow = new Color32(150, 145, 140, 255);
    private static readonly Color32 Deep = new Color32(48, 46, 45, 255);

    private readonly Color32[] cornerColors = new Color32[4];

    public SectionLayer_Ash(Section section) : base(section) {
        relevantChangeTypes = DefDatabase<MapMeshFlagDef>.GetNamed("Cosmere_Scadrial_MapMeshFlag_Ash");
    }

    public override bool Visible => AshEra.ShouldRender(Map);

    public override void Regenerate() {
        ClearSubMeshes(MeshParts.All);

        AshDepthTracker? tracker = Map.GetComponent<AshDepthTracker>();
        if (tracker == null) return;

        AshGrid grid = tracker.Grid;
        CellIndices indices = Map.cellIndices;
        CellRect cellRect = section.CellRect;

        for (int x = cellRect.minX; x <= cellRect.maxX; x++) {
            for (int z = cellRect.minZ; z <= cellRect.maxZ; z++) {
                IntVec3 cell = new IntVec3(x, 0, z);
                float depth = grid.GetFraction(indices.CellToIndex(cell));
                if (depth < 0.01f) continue;

                // Density climbs with depth. A fixed anchor rate leaves the map looking like
                // scattered patches however deep it gets, which is not what burial looks like.
                uint h = Hash(cell);
                float spread = Mathf.Clamp01(depth * 2.6f);
                if (h % 8u >= (uint)Mathf.CeilToInt(spread * 8f)) continue;

                PrintDecal(cell, h, depth);
            }
        }

        FinalizeMesh(MeshParts.All);
    }

    private void PrintDecal(IntVec3 cell, uint h, float depth) {
        // Coverage arrives well before the grid is full, or the map reads bare for most of a run.
        float coverage = Mathf.Clamp01(depth * 2.2f);

        float sizeJitter = ((h >> 8) & 0xFF) / 255f;
        float size = Mathf.Lerp(MinSize, MaxSize, coverage) * Mathf.Lerp(0.75f, 1.25f, sizeJitter);
        float rotation = (h >> 16) % 360u;

        Vector3 loc = new Vector3(
            cell.x + 0.5f + (((h >> 3) & 0xFF) / 255f - 0.5f),
            AltitudeLayer.Terrain.AltitudeFor(),
            cell.z + 0.5f + (((h >> 11) & 0xFF) / 255f - 0.5f)
        );

        Color32 tint = Color32.Lerp(Shallow, Deep, Mathf.Clamp01(depth * 1.3f));
        tint.a = (byte)(Mathf.Clamp01(coverage) * MaxOpacity * 255f);
        for (int i = 0; i < 4; i++) {
            cornerColors[i] = tint;
        }

        Material mat = SmearMats[(h >> 24) & 1u];
        Printer_Plane.PrintPlane(this, loc, Vector2.one * size, mat, rotation, false, null, cornerColors);
    }

    private static uint Hash(IntVec3 cell) {
        uint h = (uint)(cell.x * 73856093) ^ (uint)(cell.z * 19349663);
        h ^= h >> 13;
        h *= 2654435761u;
        return h ^ (h >> 16);
    }
}
