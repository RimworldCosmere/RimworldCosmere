using Cosmere.Core.Util;
using Cosmere.System.Roshar.Comp.Thing;
using Verse;

namespace Cosmere.System.Roshar.Comp.Map;

public class StormlightNetwork(Verse.Map map) : MapComponent(map) {
    private const int DistributeInterval = 60;

    private bool? cachedEnabled;
    private bool dirty = true;

    private bool enabled => ShardUtility.CachedAreAnyEnabled(ref cachedEnabled, ShardDefOf.Honor);

    public List<StormlightNetworkGrid> Networks { get; } = [];

    public void MarkDirty() {
        dirty = true;
    }

    public override void FinalizeInit() {
        base.FinalizeInit();
        dirty = true;
    }

    public override void MapComponentTick() {
        if (!enabled) return;
        if (!GenTicks.IsTickInterval(DistributeInterval)) return;

        if (dirty) {
            RebuildNetworks();
            dirty = false;
        }

        for (int i = 0; i < Networks.Count; i++) {
            Networks[i].Distribute();
        }
    }

    private void RebuildNetworks() {
        for (int i = 0; i < Networks.Count; i++) {
            StormlightNetworkGrid grid = Networks[i];
            for (int j = 0; j < grid.receivers.Count; j++) grid.receivers[j].Network = null;
            for (int j = 0; j < grid.batteries.Count; j++) grid.batteries[j].Network = null;
            for (int j = 0; j < grid.chargers.Count; j++) grid.chargers[j].Network = null;
        }

        Networks.Clear();

        HashSet<IntVec3> visited = [];
        HashSet<Verse.Thing> assignedBuildings = [];

        List<Verse.Thing> allThings = map.listerThings.AllThings;
        List<Verse.Thing> networkThings = [];

        for (int i = 0; i < allThings.Count; i++) {
            if (allThings[i].TryGetComp<StormlightNode>() != null) {
                networkThings.Add(allThings[i]);
            }
        }

        for (int i = 0; i < networkThings.Count; i++) {
            IntVec3 pos = networkThings[i].Position;
            if (visited.Contains(pos)) continue;

            StormlightNetworkGrid grid = new StormlightNetworkGrid();
            FloodFillNetwork(pos, grid, visited, assignedBuildings);

            if (grid.receivers.Count > 0 || grid.batteries.Count > 0 || grid.chargers.Count > 0) {
                Networks.Add(grid);
            }
        }
    }

    private bool HasStormlightNode(IntVec3 cell) {
        List<Verse.Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++) {
            if (things[i].TryGetComp<StormlightNode>() != null) return true;
        }

        return false;
    }

    private void FloodFillNetwork(
        IntVec3 start,
        StormlightNetworkGrid grid,
        HashSet<IntVec3> visited,
        HashSet<Verse.Thing> assignedBuildings
    ) {
        Queue<IntVec3> queue = new Queue<IntVec3>();
        queue.Enqueue(start);

        while (queue.Count > 0) {
            IntVec3 cell = queue.Dequeue();
            if (!visited.Add(cell)) continue;
            grid.conduitCells.Add(cell);

            ScanCellForBuildings(cell, grid, assignedBuildings);

            for (int d = 0; d < 4; d++) {
                IntVec3 adj = cell + GenAdj.CardinalDirections[d];
                if (!adj.InBounds(map) || visited.Contains(adj)) continue;

                if (HasStormlightNode(adj)) {
                    queue.Enqueue(adj);
                }
            }
        }
    }

    private void ScanCellForBuildings(
        IntVec3 cell,
        StormlightNetworkGrid grid,
        HashSet<Verse.Thing> assignedBuildings
    ) {
        List<Verse.Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++) {
            Verse.Thing thing = things[i];
            if (assignedBuildings.Contains(thing)) continue;

            if (thing.TryGetComp(out StormlightReceiver receiver)) {
                grid.receivers.Add(receiver);
                receiver.Network = grid;
                assignedBuildings.Add(thing);
            }
            else if (thing.TryGetComp(out StormlightBattery battery)) {
                grid.batteries.Add(battery);
                battery.Network = grid;
                assignedBuildings.Add(thing);
            }
            else if (thing.TryGetComp(out StormlightCharger charger)) {
                grid.chargers.Add(charger);
                charger.Network = grid;
                assignedBuildings.Add(thing);
            }
        }
    }
}