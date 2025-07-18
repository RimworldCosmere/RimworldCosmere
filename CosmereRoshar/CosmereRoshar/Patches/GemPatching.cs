using System.Collections.Generic;
using CosmereRoshar.Comps.Fabrials;
using CosmereRoshar.Comps.Gems;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
public static class GemCraftingPatch {
    public static IEnumerable<Thing> Postfix(
        IEnumerable<Thing> result,
        RecipeDef recipeDef,
        Pawn worker,
        List<Thing> ingredients,
        IBillGiver billGiver,
        Precept_ThingStyle precept = null,
        ThingStyleDef style = null,
        int? overrideGraphicIndex = null
    ) {
        ThingWithComps rawGem =
            ingredients.FirstOrDefault(t => CosmereRosharUtilities.rawGems.Contains(t.def)) as ThingWithComps;
        ThingWithComps cutGem =
            ingredients.FirstOrDefault(t => CosmereRosharUtilities.cutGems.Contains(t.def)) as ThingWithComps;
        if (rawGem != null) {
            CompRawGemstone compRawGemstone = rawGem.GetComp<CompRawGemstone>();
            if (compRawGemstone != null) {
                foreach (Thing? product in result) {
                    SetCutGemStats(product.TryGetComp<CompCutGemstone>(), compRawGemstone);
                    yield return product;
                }
            }
        } else if (cutGem != null) {
            CompCutGemstone compCutGemstone = cutGem.GetComp<CompCutGemstone>();
            if (compCutGemstone != null) {
                foreach (Thing? product in result) {
                    SetSphereGemStats(product.TryGetComp<CompGemSphere>(), compCutGemstone);
                    SetGemInFabrial(product.TryGetComp<CompApparelFabrialDiminisher>(), compCutGemstone);
                    yield return product;
                }
            }
        } else {
            foreach (Thing? product in result) {
                yield return product;
            }
        }
    }

    private static void SetCutGemStats(CompCutGemstone productComp, CompRawGemstone ingredientComp) {
        if (productComp != null) {
            productComp.maximumGemstoneSize = ingredientComp.gemstoneSize;
        }
    }

    private static void SetSphereGemStats(CompGemSphere productComp, CompCutGemstone ingredientComp) {
        if (productComp != null) {
            productComp.inheritGemstoneSize = ingredientComp.gemstoneSize;
            productComp.inheritGemstoneQuality = ingredientComp.gemstoneQuality;
            productComp.inheritGemstone = true;
        }
    }

    private static void SetGemInFabrial(CompApparelFabrialDiminisher productComp, CompCutGemstone ingredientComp) {
        if (productComp != null) {
            productComp.insertedGemstone = ingredientComp.parent;
        }
    }
}