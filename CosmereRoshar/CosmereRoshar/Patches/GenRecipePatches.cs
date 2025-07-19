using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Extension;
using HarmonyLib;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(GenRecipe))]
public static class GenRecipePatches {
    [HarmonyPatch(nameof(GenRecipe.MakeRecipeProducts))]
    [HarmonyPostfix]
    public static IEnumerable<Verse.Thing> PostfixMakeRecipeProducts(
        IEnumerable<Verse.Thing> result,
        Verse.Thing dominantIngredient
    ) {
        List<Verse.Thing> products = result.ToList();
        if (products.Count > 1) {
            foreach (Verse.Thing product in products) {
                yield return product;
            }

            yield break;
        }

        Verse.Thing firstResult = products[0];
        if (!firstResult.def.IsOneOf(
                ThingDefOf.Cosmere_Roshar_Thing_Chip,
                ThingDefOf.Cosmere_Roshar_Thing_Mark,
                ThingDefOf.Cosmere_Roshar_Thing_Broam
            )) {
            yield return firstResult;
            yield break;
        }

        firstResult.SetStuffDirect(dominantIngredient.Stuff);

        yield return firstResult;
    }
}