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
    private static readonly Color BorderColor = new Color(0.4f, 0.75f, 0.95f, 0.6f);
    private static readonly Color FillColor = new Color(0.4f, 0.75f, 0.95f, 0.12f);

    public static void OnClearAllMapsAndWorld() {
        zones.Clear();
    }

    public static void Register(int id, IntVec3 center, float radius, Map map) {
        ZoneEntry entry = new ZoneEntry {
            id = id,
            center = center,
            radius = radius,
            map = map,
            cells = CellsFor(center, radius, map),
        };

        for (int i = 0; i < zones.Count; i++) {
            if (zones[i].id == id) {
                zones[i] = entry;
                return;
            }
        }

        zones.Add(entry);
    }

    public static void Unregister(int id) {
        for (int i = zones.Count - 1; i >= 0; i--) {
            if (zones[i].id == id) {
                zones.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    ///     A zone never moves or resizes, so its cells settle the moment it is registered. Draw runs
    ///     every frame off MapInterfaceUpdate; rebuilding here re-tested (2r+1)^2 cells for the same answer.
    /// </summary>
    private static List<IntVec3> CellsFor(IntVec3 center, float radius, Map map) {
        int radiusCeil = (int)radius + 1;
        List<IntVec3> cells = new List<IntVec3>();
        for (int dx = -radiusCeil; dx <= radiusCeil; dx++) {
            for (int dz = -radiusCeil; dz <= radiusCeil; dz++) {
                IntVec3 cell = new IntVec3(center.x + dx, 0, center.z + dz);
                if (!cell.InBounds(map)) continue;
                if (cell.DistanceTo(center) <= radius) cells.Add(cell);
            }
        }

        return cells;
    }

    public static void Draw() {
        if (zones.Count == 0) return;

        Map currentMap = Find.CurrentMap;
        if (currentMap == null) return;

        Material fillMat = MaterialPool.MatFrom(BaseContent.WhiteTex, Verse.ShaderDatabase.Transparent, FillColor);

        for (int i = zones.Count - 1; i >= 0; i--) {
            ZoneEntry zone = zones[i];
            if (zone.map != currentMap) continue;

            for (int j = 0; j < zone.cells.Count; j++) {
                Vector3 pos = zone.cells[j].ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                Graphics.DrawMesh(MeshPool.plane10, pos, Quaternion.identity, fillMat, 0);
            }

            GenDraw.DrawFieldEdges(zone.cells, BorderColor);
        }
    }

    private struct ZoneEntry {
        public int id;
        public IntVec3 center;
        public float radius;
        public Map map;
        public List<IntVec3> cells;
    }
}
