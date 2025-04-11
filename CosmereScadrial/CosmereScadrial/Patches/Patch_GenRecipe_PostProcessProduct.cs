using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Comps;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patches {
    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class Patch_GenRecipe_PostProcessProduct {
        public static IEnumerable<Thing> Postfix(
            IEnumerable<Thing> results,
            RecipeDef recipeDef,
            Pawn worker,
            List<Thing> ingredients,
            IBillGiver billGiver
        ) {
            foreach (var thing in results) {
                if (thing.def.defName == "Cosmere_AllomanticVial") {
                    var comp = thing.TryGetComp<CompAllomanticVial>();
                    if (comp != null) {
                        comp.MetalNames = ingredients
                            .Where(i => i.def.defName != "Cosmere_Vodka" && i.def.defName != "WoodLog")
                            .Select(i => i.def.defName.Replace("Ind_", "").Replace("Aluminium", "Aluminum"))
                            .Distinct()
                            .ToList();
                    }
                }

                yield return thing;
            }
        }
    }
}