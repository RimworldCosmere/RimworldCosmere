using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(SitePartWorker_Outpost), nameof(SitePartWorker_Outpost.GenerateDefaultParams))]
public static class SitePartOutpostNullFactionGuardPatch {
    public static bool Prefix(
        SitePartWorker_Outpost __instance,
        ref SitePartParams __result,
        float myThreatPoints,
        PlanetTile tile,
        Faction faction
    ) {
        if (faction != null) return true;

        SitePartParams sitePartParams = new SitePartParams {
            randomValue = Rand.Int,
            threatPoints = __instance.def.wantsThreatPoints ? myThreatPoints : 0f,
        };
        sitePartParams.lootMarketValue =
            SitePartWorker_Outpost.ThreatPointsLootMarketValue.Evaluate(sitePartParams.threatPoints);
        __result = sitePartParams;
        return false;
    }
}