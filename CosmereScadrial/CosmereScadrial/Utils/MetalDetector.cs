using System.Collections.Generic;
using System.Linq;
using CosmereCore;
using CosmereCore.Defs;
using CosmereCore.ModExtensions;
using RimWorld;
using Verse;

namespace CosmereScadrial.Utils {
    public static class MetalDetector {
        private static readonly Dictionary<RecipeDef, bool> metalRecipeCache = new Dictionary<RecipeDef, bool>();

        public static bool IsCapableOfHavingMetal(ThingDef thingDef) {
            return thingDef?.category is ThingCategory.Item or ThingCategory.Building or ThingCategory.Pawn or ThingCategory.Projectile;
        }

        public static List<MetalDef> GetLinkedMetals(ThingDef thingDef, bool allowAluminum = false) {
            if (thingDef == null) return [];

            var metalsLinked = thingDef.GetModExtension<MetalsLinked>();
            var metalLinked = thingDef.GetModExtension<MetalLinked>();

            var metals = metalsLinked != null ? metalsLinked.metals : metalLinked != null ? [metalLinked.metal] : [];

            return allowAluminum ? metals : metals.Where(x => !x.Equals(MetalDefOf.Aluminum)).ToList();
        }

        /// <summary>
        ///     This isn't entirely accurate at getting the metal mass of an item, but its a rough implementation.
        /// </summary>
        public static float GetMetal(Thing thing, int depth = 0, bool allowAluminum = false) {
            if (thing?.def == null || depth > 25) return 0f;

            var metals = GetLinkedMetals(thing.def, allowAluminum);
            if (metals.Count > 0) return thing.GetStatValue(RimWorld.StatDefOf.Mass);
            if (thing.def.IsMetal) return thing.GetStatValue(RimWorld.StatDefOf.Mass);
            if (thing.Stuff != null && (thing.Stuff.IsMetal || GetLinkedMetals(thing.Stuff, allowAluminum).Count > 0)) return thing.GetStatValue(RimWorld.StatDefOf.Mass);
            if (thing.def.defName is "ChunkSlagSteel" or "ChunkMechanoidSlag") return thing.GetStatValue(RimWorld.StatDefOf.Mass);
            if (thing.def.stuffCategories?.Contains(StuffCategoryDefOf.Metallic) == true) return thing.GetStatValue(RimWorld.StatDefOf.Mass);

            foreach (var recipe in RecipesThatMake(thing.def)) {
                if (RecipeUsesMetalIngredient(recipe, depth + 1, allowAluminum)) return thing.GetStatValue(RimWorld.StatDefOf.Mass);
            }

            if (thing.def.category == ThingCategory.Pawn && thing is Pawn pawn) {
                var combinedMass = 0f;
                foreach (var item in pawn.inventory?.innerContainer ?? []) {
                    var metalMass = GetMetal(item, depth + 1);
                    if (metalMass > 0) combinedMass += metalMass * item.stackCount;
                }

                foreach (var item in pawn.apparel?.WornApparel ?? Enumerable.Empty<Apparel>()) {
                    var metalMass = GetMetal(item, depth + 1);
                    if (metalMass > 0) combinedMass += metalMass * item.stackCount;
                }

                foreach (var item in pawn.equipment?.AllEquipmentListForReading ?? []) {
                    var metalMass = GetMetal(item, depth + 1);
                    if (metalMass > 0) combinedMass += metalMass * item.stackCount;
                }

                if (combinedMass > 0f) return combinedMass;
            }

            if (!thing.def.killedLeavings.NullOrEmpty()) {
                return thing.def.killedLeavings.Sum(def => GetMetalForThingDefCountClass(def, depth + 1));
            }

            if (!thing.def.costList.NullOrEmpty()) {
                return thing.def.costList.Sum(def => GetMetalForThingDefCountClass(def, depth + 1));
            }

            return 0f;
        }

        public static float GetMetalForThingDefCountClass(ThingDefCountClass def, int depth) {
            var item = ThingMaker.MakeThing(def.thingDef);
            var metalMass = GetMetal(item, depth + 1);
            return metalMass * def.count;
        }

        public static List<RecipeDef> RecipesThatMake(ThingDef thingDef) {
            return DefDatabase<RecipeDef>.AllDefsListForReading
                .Where(recipe =>
                    recipe.products != null &&
                    recipe.products.Any(p => p.thingDef == thingDef))
                .ToList();
        }

        public static bool RecipeUsesMetalIngredient(RecipeDef recipe, int depth, bool allowAluminum = false) {
            if (metalRecipeCache.TryGetValue(recipe, out var metalIngredient)) return metalIngredient;

            if (recipe.ingredients == null || recipe.ingredients.Count == 0) {
                metalRecipeCache[recipe] = false;
                return false;
            }

            if (!Enumerable.Any(recipe.ingredients, ingredient => ingredient.filter.AllowedThingDefs.Any(thingDef => GetMetal(ThingMaker.MakeThing(thingDef), depth, allowAluminum) > 0f))) {
                metalRecipeCache[recipe] = false;
                return false;
            }

            metalRecipeCache[recipe] = true;
            return true;
        }
    }
}