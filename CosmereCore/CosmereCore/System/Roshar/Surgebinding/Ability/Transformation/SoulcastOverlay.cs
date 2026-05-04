using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transformation;

[HarmonyPatch]
public static class SoulcastOverlay {
    private static readonly Dictionary<int, List<OverlayEntry>> _byMap = [];

    [HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
    [HarmonyPostfix]
    public static void OnClearAllMapsAndWorld() {
        _byMap.Clear();
    }

    public static void Add(
        IntVec3 cell,
        ThingDef? materialDef,
        TerrainDef? terrainDef,
        Map map,
        SoulcastMode mode = SoulcastMode.ConvertDrop
    ) {
        Texture2D? icon = null;

        if (mode == SoulcastMode.Wall) {
            icon = RimWorld.ThingDefOf.Wall.uiIcon;
        }
        else if (mode == SoulcastMode.Sculpture) {
            icon = ThingDefOf.SculptureLarge?.uiIcon;
        }
        else if (terrainDef != null) {
            icon = ContentFinder<Texture2D>.Get(terrainDef.texturePath, false);
        }
        else if (materialDef != null) {
            icon = materialDef.uiIcon;
        }

        Entries(map).Add(
            new OverlayEntry {
                cell = cell,
                icon = icon,
                mode = mode,
                material = materialDef,
                terrain = terrainDef,
            }
        );
    }

    public static bool TryGetData(
        IntVec3 cell,
        Map map,
        out SoulcastMode mode,
        out ThingDef? material,
        out TerrainDef? terrain
    ) {
        List<OverlayEntry> entries = Entries(map);
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

    public static void Remove(IntVec3 cell, Map map) {
        List<OverlayEntry> entries = Entries(map);
        for (int i = entries.Count - 1; i >= 0; i--) {
            if (entries[i].cell == cell) {
                entries.RemoveAt(i);
                return;
            }
        }
    }

    public static void Draw(Map map) {
        List<OverlayEntry> entries = Entries(map);
        if (entries.Count == 0) return;

        for (int i = 0; i < entries.Count; i++) {
            OverlayEntry entry = entries[i];
            Vector3 pos = entry.cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);

            GenDraw.DrawTargetHighlightWithLayer(pos, AltitudeLayer.MetaOverlays);

            if (entry.icon != null) {
                Material iconMat = MaterialPool.MatFrom(
                    entry.icon,
                    Verse.ShaderDatabase.Transparent,
                    new Color(1f, 1f, 1f, 0.35f)
                );
                Vector3 iconPos = pos + new Vector3(0f, 0.1f, 0f);
                Graphics.DrawMesh(MeshPool.plane08, iconPos, Quaternion.identity, iconMat, 0);
            }
        }
    }

    private static List<OverlayEntry> Entries(Map map) {
        if (!_byMap.TryGetValue(map.uniqueID, out List<OverlayEntry>? list)) {
            list = [];
            _byMap[map.uniqueID] = list;
        }
        return list;
    }

    private struct OverlayEntry {
        public IntVec3 cell;
        public Texture2D? icon;
        public SoulcastMode mode;
        public ThingDef? material;
        public TerrainDef? terrain;
    }
}
