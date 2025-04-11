using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereCore.Patches {
    [HarmonyPatch(typeof(TraitDef), nameof(TraitDef.GetGenderSpecificCommonality))]
    public static class Patch_TraitDef_GetGenderSpecificCommonality {
        public static bool Prefix(TraitDef __instance, ref float __result, Gender gender) {
            var defName = __instance.defName;
            if (!defName.StartsWith("Cosmere_Misting_") && !defName.StartsWith("Cosmere_Ferring_") && defName != "Cosmere_FullFeruchemist" && defName != "Cosmere_Mistborn") return true;

            __result = 0f;
            return false;
        }
    }
}