using Concord;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Abrasion;

[Patch(typeof(MemoryUtility))]
public static class FrictionTrapOverlayStateClearer {
    [Inject(At.Return, nameof(MemoryUtility.ClearAllMapsAndWorld))]
    private static void AfterClearAllMapsAndWorld() {
        FrictionTrapOverlay.OnClearAllMapsAndWorld();
    }
}

public static class FrictionTrapOverlay {
    private static readonly List<ZoneEntry> zones = [];
    private static readonly List<IntVec3> cellBuffer = [];
    private static readonly Color BorderColor = new Color(0.4f, 0.75f, 0.95f, 0.6f);
    private static readonly Color FillColor = new Color(0.4f, 0.75f, 0.95f, 0.12f);

    public static void OnClearAllMapsAndWorld() {
        zones.Clear();
        cellBuffer.Clear();
    }

    public static void Register(int id, IntVec3 center, float radius, Map map) {
        for (int i = 0; i < zones.Count; i++) {
            if (zones[i].id == id) {
                zones[i] = new ZoneEntry { id = id, center = center, radius = radius, map = map };
                return;
            }
        }

        zones.Add(new ZoneEntry { id = id, center = center, radius = radius, map = map });
    }

    public static void Unregister(int id) {
        for (int i = zones.Count - 1; i >= 0; i--) {
            if (zones[i].id == id) {
                zones.RemoveAt(i);
                return;
            }
        }
    }

    public static void Draw() {
        if (zones.Count == 0) return;

        Map currentMap = Find.CurrentMap;
        if (currentMap == null) return;

        Material fillMat = MaterialPool.MatFrom(BaseContent.WhiteTex, Verse.ShaderDatabase.Transparent, FillColor);

        for (int i = zones.Count - 1; i >= 0; i--) {
            ZoneEntry zone = zones[i];
            if (zone.map != currentMap) continue;

            cellBuffer.Clear();
            int radiusCeil = (int)zone.radius + 1;
            for (int dx = -radiusCeil; dx <= radiusCeil; dx++) {
                for (int dz = -radiusCeil; dz <= radiusCeil; dz++) {
                    IntVec3 cell = new IntVec3(zone.center.x + dx, 0, zone.center.z + dz);
                    if (!cell.InBounds(currentMap)) continue;
                    if (cell.DistanceTo(zone.center) <= zone.radius) {
                        cellBuffer.Add(cell);
                    }
                }
            }

            for (int j = 0; j < cellBuffer.Count; j++) {
                Vector3 pos = cellBuffer[j].ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                Graphics.DrawMesh(MeshPool.plane10, pos, Quaternion.identity, fillMat, 0);
            }

            GenDraw.DrawFieldEdges(cellBuffer, BorderColor);
        }
    }

    private struct ZoneEntry {
        public int id;
        public IntVec3 center;
        public float radius;
        public Map map;
    }
}
