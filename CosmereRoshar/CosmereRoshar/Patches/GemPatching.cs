using System.Collections.Generic;
using CosmereRoshar.Comp.Fabrials;
using CosmereRoshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
public static class GemCraftingPatch {
    public static IEnumerable<Verse.Thing> Postfix(
        IEnumerable<Verse.Thing> result,
        RecipeDef recipeDef,
        Pawn worker,
        List<Verse.Thing> ingredients,
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
            RawGemstone rawGemstone = rawGem.GetComp<RawGemstone>();
            if (rawGemstone != null) {
                foreach (Verse.Thing? product in result) {
                    SetCutGemStats(product.TryGetComp<CompCutGemstone>(), rawGemstone);
                    yield return product;
                }
            }
        } else if (cutGem != null) {
            CompCutGemstone compCutGemstone = cutGem.GetComp<CompCutGemstone>();
            if (compCutGemstone != null) {
                foreach (Verse.Thing? product in result) {
                    SetSphereGemStats(product.TryGetComp<GemSphere>(), compCutGemstone);
                    SetGemInFabrial(product.TryGetComp<FabrialDiminisher>(), compCutGemstone);
                    yield return product;
                }
            }
        } else {
            foreach (Verse.Thing? product in result) {
                yield return product;
            }
        }
    }

    private static void SetCutGemStats(CompCutGemstone? productComp, RawGemstone ingredient) {
        if (productComp != null) {
            productComp.maximumGemstoneSize = ingredient.gemstoneSize;
        }
    }

    private static void SetSphereGemStats(GemSphere? product, CompCutGemstone ingredientComp) {
        if (product == null) return;
        product.inheritGemstoneSize = ingredientComp.gemstoneSize;
        product.inheritGemstoneQuality = ingredientComp.gemstoneQuality;
        product.inheritGemstone = true;
    }

    private static void SetGemInFabrial(FabrialDiminisher? productComp, CompCutGemstone ingredientComp) {
        if (productComp != null) {
            productComp.insertedGemstone = ingredientComp.parent;
        }
    }
}