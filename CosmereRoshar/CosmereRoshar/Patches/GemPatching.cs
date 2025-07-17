using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CosmereRoshar {
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class GemCraftingPatch {

        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, IBillGiver billGiver, Precept_ThingStyle precept = null, ThingStyleDef style = null, int? overrideGraphicIndex = null) {
            ThingWithComps rawGem = ingredients.FirstOrDefault(t => CosmereRosharUtilities.RawGems.Contains(t.def)) as ThingWithComps;
            ThingWithComps cutGem = ingredients.FirstOrDefault(t => CosmereRosharUtilities.CutGems.Contains(t.def)) as ThingWithComps;
            if (rawGem != null) {
                CompRawGemstone compRawGemstone = rawGem.GetComp<CompRawGemstone>();
                if (compRawGemstone != null) {
                    foreach (var product in __result) {
                        setCutGemStats(product.TryGetComp<CompCutGemstone>(), compRawGemstone);
                        yield return product;
                    }
                }
            }
            else if (cutGem != null) {
                CompCutGemstone compCutGemstone = cutGem.GetComp<CompCutGemstone>();
                if (compCutGemstone != null) {
                    foreach (var product in __result) {
                        setSphereGemStats(product.TryGetComp<CompGemSphere>(), compCutGemstone);
                        setGemInFabrial(product.TryGetComp<CompApparelFabrialDiminisher>(), compCutGemstone);
                        yield return product;
                    }
                }
            }
            else {
                foreach (var product in __result) {
                    yield return product;
                }
            }
        }

        private static void setCutGemStats(CompCutGemstone productComp, CompRawGemstone ingredientComp) {
            if (productComp != null) {
                productComp.maximumGemstoneSize = ingredientComp.gemstoneSize;
            }
        }
        private static void setSphereGemStats(CompGemSphere productComp, CompCutGemstone ingredientComp) {
            if (productComp != null) {
                productComp.inheritGemstoneSize = ingredientComp.gemstoneSize;
                productComp.inheritGemstoneQuality = ingredientComp.gemstoneQuality;
                productComp.inheritGemstone = true;
            }
        }

        private static void setGemInFabrial(CompApparelFabrialDiminisher productComp, CompCutGemstone ingredientComp) {
            if (productComp != null) {
                productComp.insertedGemstone = ingredientComp.parent;
            }
        }
    }
}


