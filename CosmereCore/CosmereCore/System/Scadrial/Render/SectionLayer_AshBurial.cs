using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Grid;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Render;

/// <summary>
///     Ash deep enough to bury what is under it. Drawn at ItemImportant so it covers plants and
///     dropped items but leaves pawns visible - two and a half metres of ash should swallow a
///     stockpile, not hide the colonist standing next to it.
/// </summary>
[StaticConstructorOnStartup]
public class SectionLayer_AshBurial : SectionLayer {
    private static readonly Material AshMat =
        MaterialPool.MatFrom("Terrain/AshDeep", Verse.ShaderDatabase.Transparent);

    private static readonly Color32 Deep = new Color32(58, 55, 53, 255);

    private const float MaxOpacity = 0.94f;

    private readonly Color32[] cornerColors = new Color32[4];

    public SectionLayer_AshBurial(Section section) : base(section) {
        relevantChangeTypes = DefDatabase<MapMeshFlagDef>.GetNamed("Cosmere_Scadrial_MapMeshFlag_Ash");
    }

    public override bool Visible => AshEra.ShouldRender(Map);

    public override void Regenerate() {
        ClearSubMeshes(MeshParts.All);

        AshDepthTracker? tracker = Map.GetComponent<AshDepthTracker>();
        if (tracker == null) return;

        AshBuriedCells buried = tracker.Buried;
        if (!buried.Any) return;

        AshGrid grid = tracker.Grid;
        CellIndices indices = Map.cellIndices;
        CellRect cellRect = section.CellRect;
        float altitude = AltitudeLayer.ItemImportant.AltitudeFor();

        for (int x = cellRect.minX; x <= cellRect.maxX; x++) {
            for (int z = cellRect.minZ; z <= cellRect.maxZ; z++) {
                int index = indices.CellToIndex(new IntVec3(x, 0, z));

                // The set, not the depth. Recomputing here would drop the hysteresis.
                if (!buried.IsBuried(index)) continue;

                // Ramps from nothing at the unbury line to near-solid at the cap.
                float t = Mathf.Clamp01(
                    (grid.GetDepthMm(index) - AshDepthMath.UncoveredMm) /
                    (float)(AshGrid.MaxDepthMm - AshDepthMath.UncoveredMm)
                );

                Color32 tint = Deep;
                tint.a = (byte)(t * MaxOpacity * 255f);
                for (int i = 0; i < 4; i++) {
                    cornerColors[i] = tint;
                }

                Vector3 loc = new Vector3(x + 0.5f, altitude, z + 0.5f);
                Printer_Plane.PrintPlane(this, loc, Vector2.one * 1.02f, AshMat, 0f, false, null, cornerColors);
            }
        }

        FinalizeMesh(MeshParts.All);
    }
}
