using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereFramework.Patch;

[HarmonyPatch]
public static class PatchResearchWindowHeaderLabel {
    [HarmonyPatch(typeof(MainTabWindow_Research), "HeaderLabel")]
    [HarmonyPostfix]
    public static bool Prefix(ResearchPrerequisitesUtility.UnlockedHeader headerProject, ref string __result) {
        __result = string.Join(", ",
            headerProject.unlockedBy.Select<ResearchProjectDef, object>(rp =>
                rp.IsFinished
                    ? rp.LabelCap
                    : rp.LabelCap.Colorize(ColorLibrary.RedReadable)
            ));

        return false; // Skip original method
    }
}