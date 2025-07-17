using CosmereResources.Def;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereResources.Patches;

[HarmonyPatch]
public static class GemSpawnAfterMine {
    private static ThingDef randomGemDef =>
        DefDatabase<GemDef>.AllDefsListForReading
            .RandomElementByWeight(x => x.MineableItem.building.mineableScatterCommonality)
            .Item;

    [HarmonyPatch(typeof(Mineable), "TrySpawnYield")]
    [HarmonyPatch([typeof(Map), typeof(bool), typeof(Pawn)])]
    private static void PostfixMineableTrySpawnYield(Map? map, bool moteOnWaste, Pawn pawn) {
        if (map == null) return;

        GenPlace.TryPlaceThing(ThingMaker.MakeThing(randomGemDef), pawn.Position, map, ThingPlaceMode.Direct);
    }
}