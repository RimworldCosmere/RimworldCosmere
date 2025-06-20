using System.Linq;
using CosmereResources.Defs;
using CosmereResources.ModExtensions;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patches;

[HarmonyPatch]
public static class StatsReportUtility {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RimWorld.StatsReportUtility), "DescriptionEntry", typeof(Def))]
    public static bool RemoveTokens(Def def, ref StatDrawEntry __result) {
        if (def is not GeneDef geneDef) return false;

        MetalDef metal = geneDef.GetModExtension<MetalsLinked>().Metals.First()!;
        TaggedString description =
            geneDef.description.Formatted("the current pawn".Named("PAWN"), metal.Named("METAL"));
        __result = new StatDrawEntry(StatCategoryDefOf.BasicsImportant, (string)"Description".Translate(), "",
            description, 99999,
            hyperlinks: Dialog_InfoCard.DefsToHyperlinks(def.descriptionHyperlinks));

        return false;
    }
}