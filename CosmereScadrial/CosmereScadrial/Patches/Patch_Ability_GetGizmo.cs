namespace CosmereScadrial.Patches {
    /*
        [HarmonyPatch(typeof(Ability), nameof(Ability.GetGizmo))]
        public static class Patch_Ability_GetGizmo {
            public static void Postfix(Ability __instance, ref Gizmo __result) {
                // Replace only your special abilities
                if (__instance.def.defName.StartsWith("Cosmere_Ability_Burn")) {
                    __result = new ToggleBurnMetal(__instance, __instance.pawn);
                }
            }
        }*/
}