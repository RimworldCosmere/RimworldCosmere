using System.Linq;
using Cosmere.Resources.Def;
using Cosmere.Resources.DefModExtension;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Patch;

[HarmonyPatch]
public static class StatsReportUtility {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RimWorld.StatsReportUtility), "DescriptionEntry", typeof(Verse.Def))]
    public static bool RemoveTokens(Verse.Def def, ref StatDrawEntry __result) {
        if (def is not GeneDef geneDef) return true;

        MetalDef metal = geneDef.GetModExtension<MetalsLinked>().Metals.First()!;
        TaggedString description =
            geneDef.description.Formatted("the current pawn".Named("PAWN"), metal.Named("METAL"));
        __result = new StatDrawEntry(
            StatCategoryDefOf.BasicsImportant,
            (string)"Description".Translate(),
            "",
            description,
            99999,
            hyperlinks: Dialog_InfoCard.DefsToHyperlinks(def.descriptionHyperlinks)
        );

        return false;
    }
}