using Cosmere.Def;
using Cosmere.DefModExtension;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Resources.Patches;

[HarmonyPatch]
public static class GemSpawnAfterMine {
    private static ThingDef randomGemDef =>
        DefDatabase<GemDef>.AllDefsListForReading
            .RandomElementByWeight(x => x.MineableItem.building.mineableScatterCommonality)
            .Item;

    [HarmonyPatch(typeof(Mineable), "TrySpawnYield")]
    [HarmonyPatch([typeof(Map), typeof(bool), typeof(Pawn)])]
    private static void PostfixMineableTrySpawnYield(Mineable __instance, Map? map, bool moteOnWaste, Pawn pawn) {
        if (map == null) return;
        if (__instance.def.HasModExtension<GemsLinked>()) return;

        GenPlace.TryPlaceThing(ThingMaker.MakeThing(randomGemDef), pawn.Position, map, ThingPlaceMode.Direct);
    }
}