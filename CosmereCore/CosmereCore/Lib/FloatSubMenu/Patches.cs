using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FloatSubMenus {
    [HarmonyPatch]
    internal static class Patches {
        private static bool replaceDist = false;
        private static float dist;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FloatMenu), "UpdateBaseColor")]
        public static void UpdateBaseColor_Pre(FloatMenu __instance) {
            // Make sure we do not replace any values needed to calculate replacement.
            replaceDist = false;
            replaceDist = FloatSubMenu.ShouldReplaceDistanceFor(__instance, ref dist);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FloatMenu), "UpdateBaseColor")]
        public static void UpdateBaseColor_Post() {
            replaceDist = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GenUI), nameof(GenUI.DistFromRect))]
        public static bool DistFromRect_Pre(ref float __result) {
            if (replaceDist) {
                __result = dist;
                return false;
            } else {
                return true;
            }
        }
    }
}
