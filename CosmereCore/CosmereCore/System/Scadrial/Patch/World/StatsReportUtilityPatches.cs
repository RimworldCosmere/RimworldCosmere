using Cosmere.Core.Def;
using Cosmere.Core.DefModExtension;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.World;

[HarmonyPatch]
public static class StatsReportUtilityPatch {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RimWorld.StatsReportUtility), "DescriptionEntry", typeof(Verse.Def))]
    public static bool RemoveTokens(Verse.Def def, ref StatDrawEntry __result) {
        if (def is not GeneDef geneDef) return true;

        MetalsLinked? extension = geneDef.GetModExtension<MetalsLinked>();
        if (extension == null) return true;
        MetalDef? metal = extension.Metals?.FirstOrDefault();
        if (metal == null) return true;
        TaggedString description =
            geneDef.description.Formatted("the current pawn".Named("PAWN"), metal.Named("METAL"));
        __result = new StatDrawEntry(
            StatCategoryDefOf.BasicsImportant,
            (string)"Description".Translate(),
            string.Empty,
            description,
            99999,
            hyperlinks: Dialog_InfoCard.DefsToHyperlinks(def.descriptionHyperlinks)
        );

        return false;
    }
}
