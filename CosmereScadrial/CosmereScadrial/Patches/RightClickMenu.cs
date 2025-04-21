using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Abilities;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.FloatMenu;
using FloatSubMenus;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Patches {
    [HarmonyPatch]
    public static class RightClickMenu {
        private static List<FloatMenuOption> cache;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.ChoicesAtFor))]
        public static void PostfixFloatMenuMakerMapChoicesAtFor(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> __result) {
            if (cache != null) {
                __result = cache;
                return;
            }

            var metalBurning = pawn.GetComp<MetalBurning>();
            var target = GenUI.TargetsAt(clickPos, TargetingParameters.ForPawns(), true).FirstOrDefault();
            if (!target.IsValid) return;

            var allomanticAbilities = pawn.abilities.AllAbilitiesForReading.Where(x => x.def is AllomanticAbilityDef && x.CanApplyOn(target)).Select(x => x as AllomanticAbility).ToArray();
            if (allomanticAbilities.Length == 0) return;
            var metals = allomanticAbilities.GroupBy(x => x.metal);

            Log.Verbose($"Creating menu item for Pawn={pawn} -> Target={target}. Abilities => {string.Join(", ", allomanticAbilities.Select(x => x.def.defName))}");

            var subOptions = new List<FloatMenuOption> {
                new FloatMenuSearch(true),
            };
            if (pawn.Equals(target.Pawn)) {
                subOptions.Add(new FloatMenuOption("Stop Burning All Metals", metalBurning.IsBurning() ? () => metalBurning.RemoveAllBurnSources() : null));
            }

            foreach (var metal in metals) {
                if (metal.Count() == 1) {
                    subOptions.Add(new AllomanticAbilityFloatMenuOption(metal.First(), target));
                } else {
                    var metalMenu = new FloatSubMenu(metal.Key.LabelCap, new List<FloatMenuOption>());
                    foreach (var ability in metal) {
                        metalMenu.Options.Add(new AllomanticAbilityFloatMenuOption(ability, target));
                    }

                    subOptions.Add(metalMenu);
                }
            }

            __result.Add(new FloatSubMenu("Allomancy", subOptions));
            cache = __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Verse.FloatMenu), nameof(Verse.FloatMenu.PostClose))]
        public static void PostfixFloatMenuPostClose() {
            cache = null;
        }
    }
}