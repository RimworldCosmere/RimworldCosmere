using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Abilities.Allomancy;
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
            var targetParams = TargetingParameters.ForAttackAny();
            targetParams.canTargetSelf = true;
            targetParams.canTargetLocations = true;
            targetParams.canTargetItems = true;
            targetParams.canTargetPlants = true;
            targetParams.mapObjectTargetsMustBeAutoAttackable = false;

            var subOptions = new List<FloatMenuOption>();
            var allomancyMenu = new FloatSubMenu("Allomancy", subOptions);
            var targets = GenUI.TargetsAt(clickPos, targetParams, true).ToArray();

            var targetMenus = new Dictionary<LocalTargetInfo, List<FloatMenuOption>>();
            foreach (var target in targets) {
                var subTargetOptions = GetSubTargetOptions(pawn, metalBurning, target);
                if (subTargetOptions == null) continue;
                targetMenus[target] = subTargetOptions;
            }

            if (targetMenus.Count == 1) {
                subOptions.AddRange(targetMenus.First().Value);
            } else if (targetMenus.Count > 1) {
                foreach (var (key, value) in targetMenus) {
                    subOptions.Add(new FloatSubMenu($"Target: {key.Thing.LabelCap}", value));
                }
            } else {
                return;
            }

            __result.Add(allomancyMenu);
        }

        private static List<FloatMenuOption> GetSubTargetOptions(Pawn pawn, MetalBurning metalBurning, LocalTargetInfo target) {
            if (!target.IsValid) return null;

            var allomanticAbilities = pawn.abilities.AllAbilitiesForReading.Where(x => x.def is AllomanticAbilityDef && x.CanApplyOn(target)).Select(x => x as AbstractAbility).ToArray();
            if (allomanticAbilities.Length == 0) return null;
            var metals = allomanticAbilities.GroupBy(x => x.metal);

            var options = new List<FloatMenuOption> {
                new FloatMenuSearch(true),
            };
            if (pawn.Equals(target.Pawn)) {
                options.Add(new FloatMenuOption("Stop Burning All Metals", metalBurning.IsBurning() ? () => metalBurning.RemoveAllBurnSources() : null));
            }

            foreach (var metal in metals) {
                if (metal.Count() == 1) {
                    options.Add(new AllomanticAbilityFloatMenuOption(metal.First(), target));
                } else {
                    var metalMenu = new FloatSubMenu(metal.Key.LabelCap, new List<FloatMenuOption>());
                    foreach (var ability in metal) {
                        metalMenu.Options.Add(new AllomanticAbilityFloatMenuOption(ability, target));
                    }

                    options.Add(metalMenu);
                }
            }

            return options;
        }
    }
}