using System.Collections.Generic;
using System.Linq;
using CosmereMetals;
using CosmereMetals.Defs;
using CosmereMetals.ModExtensions;
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

            var metals = thingDef.GetModExtension<MetalsLinked>()?.Metals ?? [];

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
            //if (thing.Stuff != null && (thing.Stuff.IsMetal || GetLinkedMetals(thing.Stuff, allowAluminum).Count > 0)) return thing.GetStatValue(RimWorld.StatDefOf.Mass);
            if (thing.def.defName is "ChunkSlagSteel" or "ChunkMechanoidSlag") return thing.GetStatValue(RimWorld.StatDefOf.Mass);

            if (thing.Stuff != null) {
                if (thing.Stuff.IsMetal || GetLinkedMetals(thing.Stuff, allowAluminum).Count > 0) {
                    var mass = thing.Stuff.GetStatValueAbstract(RimWorld.StatDefOf.Mass);

                    return mass * thing.def.CostStuffCount;
                }
            }

            if (thing.def.defName.Contains("Ancient")) {
                if (thing.def.defName != "AncientFence") return thing.GetStatValue(RimWorld.StatDefOf.Mass);
            }

            if (RecipesThatMake(thing.def).Any(recipe => RecipeUsesMetalIngredient(recipe, depth + 1, allowAluminum))) return thing.GetStatValue(RimWorld.StatDefOf.Mass);

            if (thing.def.category == ThingCategory.Pawn && thing is Pawn pawn) {
                var combinedMass = (pawn.inventory?.innerContainer ?? []).Sum(item => GetMetal(item, depth + 1) * item.stackCount);
                combinedMass += (pawn.apparel?.WornApparel ?? []).Sum(item => GetMetal(item, depth + 1) * item.stackCount);
                combinedMass += (pawn.equipment?.AllEquipmentListForReading ?? []).Sum(item => GetMetal(item, depth + 1) * item.stackCount);

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


        public static float GetMass(Thing thing) {
            var massStat = thing.GetStatValue(RimWorld.StatDefOf.Mass);
            switch (thing?.def?.category) {
                case ThingCategory.Pawn:
                    var pawn = (Pawn)thing;
                    var bodyType = pawn.story?.bodyType;
                    var size = 1f;

                    if (bodyType == null) {
                        size = 0.7f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Thin)) {
                        size = 0.8f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Baby)) {
                        size = 0.2f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Child)) {
                        size = 0.5f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Fat)) {
                        size = 1.3f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Hulk)) {
                        size = 1.5f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Female)) {
                        size = 0.9f;
                    }

                    return massStat * size + GetMetal(thing);
                case ThingCategory.Item:
                    return massStat * thing.stackCount;
                case ThingCategory.Building: {
                    var building = (Building)thing;
                    if (massStat <= 1f) {
                        massStat = 100;
                    }

                    // Add a 100x multiplier to this stat. For some reason, buildings don't have a mass.
                    return massStat * building.def.Size.x * building.def.Size.z * GetMetal(building);
                }
                case null: return float.MaxValue;
                default:
                    return massStat;
            }
        }
    }
}