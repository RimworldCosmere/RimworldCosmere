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

namespace CosmereScadrial.Patches {
    [HarmonyPatch]
    public static class RightClickMenu {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.ChoicesAtFor))]
        public static void PostfixFloatMenuMakerMapChoicesAtFor(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> __result) {
            var metalBurning = pawn.GetComp<MetalBurning>();
            var target = GenUI.TargetsAt(clickPos, TargetingParameters.ForPawns(), true).FirstOrDefault();
            if (!target.IsValid) return;

            var allomanticAbilities = pawn.abilities.AllAbilitiesForReading.Where(x => x.def is AllomanticAbilityDef && x.CanApplyOn(target)).Select(x => x as AbstractAllomanticAbility).ToArray();
            if (allomanticAbilities.Length == 0) return;
            var metals = allomanticAbilities.GroupBy(x => x.metal);

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
        }
    }
}