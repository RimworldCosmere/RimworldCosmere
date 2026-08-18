using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quest;

/// <summary>
///     Counts an item across the colony's home maps, inventories included. A
///     caravan hands its cargo over inside its pawns and they unload it over the following
///     while, so anything reading only map.listerThings sees whatever hauling has caught up
///     with rather than what actually came home.
/// </summary>
public static class HomeStock {
    public static int Count(ThingDef? def) {
        List<Verse.Thing> found = new List<Verse.Thing>();
        Collect(def, found);

        int count = 0;
        for (int i = 0; i < found.Count; i++) {
            count += found[i].stackCount;
        }

        return count;
    }

    private static void Collect(ThingDef? def, List<Verse.Thing> outThings) {
        if (def == null) return;

        Faction? player = Faction.OfPlayer;
        List<Verse.Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            Verse.Map map = maps[i];
            if (!map.IsPlayerHome) continue;

            List<Verse.Thing> spawned = map.listerThings.ThingsOfDef(def);
            for (int j = 0; j < spawned.Count; j++) {
                outThings.Add(spawned[j]);
            }

            // Player pawns only: a visiting trader's pack animals aren't the colony's to count.
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int j = 0; j < pawns.Count; j++) {
                Pawn pawn = pawns[j];
                if (pawn.Faction != player) continue;

                ThingOwner<Verse.Thing>? inventory = pawn.inventory?.innerContainer;
                if (inventory != null) {
                    for (int k = 0; k < inventory.Count; k++) {
                        if (inventory[k].def == def) outThings.Add(inventory[k]);
                    }
                }

                Verse.Thing? carried = pawn.carryTracker?.CarriedThing;
                if (carried != null && carried.def == def) outThings.Add(carried);
            }
        }
    }
}
