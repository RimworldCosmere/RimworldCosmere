using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transformation;

public static class SoulcastOverlay {
    private static readonly List<OverlayEntry> entries = [];

    public static void Add(IntVec3 cell, ThingDef? materialDef, TerrainDef? terrainDef, SoulcastMode mode = SoulcastMode.ConvertDrop) {
        Texture2D? icon = null;

        if (mode == SoulcastMode.Wall) {
            icon = RimWorld.ThingDefOf.Wall.uiIcon;
        } else if (mode == SoulcastMode.Sculpture) {
            icon = DefDatabase<ThingDef>.GetNamedSilentFail("SculptureLarge")?.uiIcon;
        } else if (terrainDef != null) {
            icon = ContentFinder<Texture2D>.Get(terrainDef.texturePath, false);
        } else if (materialDef != null) {
            icon = materialDef.uiIcon;
        }

        entries.Add(new OverlayEntry {
            cell = cell,
            icon = icon,
            mode = mode,
            material = materialDef,
            terrain = terrainDef,
        });
    }

    public static bool TryGetData(IntVec3 cell, out SoulcastMode mode, out ThingDef? material, out TerrainDef? terrain) {
        for (int i = 0; i < entries.Count; i++) {
            if (entries[i].cell == cell) {
                mode = entries[i].mode;
                material = entries[i].material;
                terrain = entries[i].terrain;
                return true;
            }
        }
        mode = default;
        material = null;
        terrain = null;
        return false;
    }

    public static void Remove(IntVec3 cell) {
        for (int i = entries.Count - 1; i >= 0; i--) {
            if (entries[i].cell == cell) {
                entries.RemoveAt(i);
                return;
            }
        }
    }

    public static void Clear() {
        entries.Clear();
    }

    public static void Draw() {
        if (entries.Count == 0) return;

        for (int i = 0; i < entries.Count; i++) {
            OverlayEntry entry = entries[i];
            Vector3 pos = entry.cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);

            GenDraw.DrawTargetHighlightWithLayer(pos, AltitudeLayer.MetaOverlays);

            if (entry.icon != null) {
                Material iconMat = MaterialPool.MatFrom(entry.icon, Verse.ShaderDatabase.Transparent, new Color(1f, 1f, 1f, 0.35f));
                Vector3 iconPos = pos + new Vector3(0f, 0.1f, 0f);
                Graphics.DrawMesh(MeshPool.plane08, iconPos, Quaternion.identity, iconMat, 0);
            }
        }
    }

    private struct OverlayEntry {
        public IntVec3 cell;
        public Texture2D? icon;
        public SoulcastMode mode;
        public ThingDef? material;
        public TerrainDef? terrain;
    }
}
