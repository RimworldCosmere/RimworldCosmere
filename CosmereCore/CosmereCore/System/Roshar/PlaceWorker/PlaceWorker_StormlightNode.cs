using Cosmere.System.Roshar.Comp.Map;
using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.PlaceWorker;

public class PlaceWorker_StormlightNode : Verse.PlaceWorker {
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Verse.Thing? thing = null) {
        StormlightOverlayDrawHandler.DrawThisFrame();

        Map map = Find.CurrentMap;
        if (map == null) return;

        StormlightNetwork? network = map.GetComponent<StormlightNetwork>();
        if (network == null) return;

        IntVec3 connectFrom = center;
        if (def.building?.isAttachment == true) {
            Verse.Thing? wall = GenConstruct.GetWallAttachedTo(center, rot, map);
            if (wall != null) connectFrom = wall.Position;
        }

        IntVec3? nearest = FindNearestConduitCell(network, connectFrom, def);
        if (!nearest.HasValue) return;

        Vector3 buildingCenter = GenThing.TrueCenter(center, rot, def.size, AltitudeLayer.MapDataOverlay.AltitudeFor());
        Vector3 conduitCenter = nearest.Value.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay);

        if (center == nearest.Value) return;

        Vector3 mid = (buildingCenter + conduitCenter) / 2f;
        Vector3 diff = conduitCenter - buildingCenter;
        Vector3 scale = new Vector3(1f, 1f, diff.MagnitudeHorizontal());
        Quaternion rotation = Quaternion.LookRotation(diff);
        Matrix4x4 matrix = default;
        matrix.SetTRS(mid, rotation, scale);
        Graphics.DrawMesh(MeshPool.plane10, matrix, StormlightNode.MatConnectorAnticipated, 0);
    }

    private static IntVec3? FindNearestConduitCell(StormlightNetwork network, IntVec3 pos, ThingDef def) {
        int range = 0;
        CompProperties? props = def.GetCompProperties<StormlightNodeProperties>();
        if (props is StormlightNodeProperties nodeProps) range = nodeProps.connectRange;
        props ??= def.GetCompProperties<StormlightChargerProperties>();
        if (props is StormlightChargerProperties chargerProps) range = chargerProps.connectRange;

        float bestDistSq = float.MaxValue;
        IntVec3? bestCell = null;

        List<StormlightNetworkGrid> networks = network.Networks;
        for (int i = 0; i < networks.Count; i++) {
            StormlightNetworkGrid grid = networks[i];
            foreach (IntVec3 cell in grid.conduitCells) {
                float distSq = (cell - pos).LengthHorizontalSquared;
                if (distSq < bestDistSq && (range <= 0 || distSq <= range * range)) {
                    bestDistSq = distSq;
                    bestCell = cell;
                }
            }
        }

        return bestCell;
    }
}
