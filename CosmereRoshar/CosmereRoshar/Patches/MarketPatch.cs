using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using CosmereRoshar;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;

namespace CosmereRoshar {

        [HarmonyPatch(typeof(StockGenerator_MiscItems))]
        [HarmonyPatch(nameof(StockGenerator_MiscItems.GenerateThings))]
        [HarmonyPatch(new[] { typeof(PlanetTile), typeof(Faction) })]
        public static class StockGenerator_MiscItems_GenerateThings_Patch {
            // __0 = PlanetTile forTile   (unused here)
            // __1 = Faction faction
            static void Postfix(ref IEnumerable<Thing> __result,
                                PlanetTile __0,
                                Faction __1) {
                var things = __result.ToList();

                // add Pewter bars 25 % of the time if not already present
                if (!things.Any(t => t.def == CosmereRosharDefs.whtwl_AlloyPewter) &&
                    StormlightUtilities.RollTheDice(1, 4, 2)) {
                    int stack = StormlightUtilities.RollTheDice(1, 20);
                    Thing pewter = StockGeneratorUtility.TryMakeForStockSingle(
                                       CosmereRosharDefs.whtwl_AlloyPewter, stack, __1);
                    if (pewter != null) things.Add(pewter);
                }

                __result = things;
            }
        }
}