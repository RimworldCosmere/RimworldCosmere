using System.Collections.Generic;
using System.Linq;
using CosmereResources;
using CosmereResources.Def;
using CosmereResources.DefModExtension;
using RimWorld;
using Verse;
using ThingDefOf = CosmereResources.ThingDefOf;

namespace CosmereCore.Util;

public static class MetalDetector {
    private static readonly Dictionary<RecipeDef, bool> MetalRecipeCache = new Dictionary<RecipeDef, bool>();
    private static readonly Dictionary<Thing, float> MetalThingCache = new Dictionary<Thing, float>();

    public static bool IsCapableOfHavingMetal(ThingDef? thingDef) {
        return thingDef?.category is ThingCategory.Item
            or ThingCategory.Building
            or ThingCategory.Pawn
            or ThingCategory.Projectile;
    }

    public static List<MetalDef> GetLinkedMetals(ThingDef? thingDef, bool allowAluminum = false) {
        if (thingDef == null) return [];

        List<MetalDef> metals = thingDef.GetModExtension<MetalsLinked>()?.Metals ?? [];

        return allowAluminum ? metals : metals.Where(x => !x.Equals(MetalDefOf.Aluminum)).ToList();
    }

    public static bool HasMetal(Thing? thing, int depth = 0, bool allowAluminum = false) {
        return GetMetal(thing, depth, allowAluminum) > 0f;
    }

    private static float CalculateMetal(Thing? thing, int depth = 0, bool allowAluminum = false) {
        if (!IsCapableOfHavingMetal(thing?.def)) return 0f;
        if (thing?.def == null || depth > 25) return 0f;

        List<MetalDef> metals = GetLinkedMetals(thing.def, allowAluminum);
        if (metals.Count > 0 || thing.def.IsMetal) {
            if (thing.def.category != ThingCategory.Building) {
                return thing.GetStatValue(RimWorld.StatDefOf.Mass);
            }

            if (thing.def.defName.StartsWith("Mineable")) {
                return thing.def.building.mineableYield *
                       10 *
                       metals.First().Item.GetStatValueAbstract(RimWorld.StatDefOf.Mass);
            }
        }

        //if (thing.Stuff != null && (thing.Stuff.IsMetal || GetLinkedMetals(thing.Stuff, allowAluminum).Count > 0)) return thing.GetStatValue(RimWorld.StatDefOf.Mass);
        if (thing.def.defName is "ChunkSlagSteel" or "ChunkMechanoidSlag") {
            return thing.GetStatValue(RimWorld.StatDefOf.Mass);
        }

        if (thing.Stuff != null) {
            if (thing.Stuff.IsMetal && !thing.Stuff.Equals(ThingDefOf.Aluminum) ||
                GetLinkedMetals(thing.Stuff, allowAluminum).Count > 0) {
                float mass = thing.Stuff.GetStatValueAbstract(RimWorld.StatDefOf.Mass, thing.Stuff);

                return mass * thing.def.CostStuffCount;
            }
        }

        if (thing.def.defName.Contains("Ancient")) {
            if (thing.def.defName != "AncientFence") return 1;
        }

        if (RecipesThatMake(thing.def).Any(recipe => RecipeUsesMetalIngredient(recipe, depth + 1, allowAluminum))) {
            return thing.GetStatValue(RimWorld.StatDefOf.Mass);
        }

        if (thing.def.category == ThingCategory.Pawn && thing is Pawn pawn) {
            float combinedMass =
                (pawn.inventory?.innerContainer ?? []).Sum(item =>
                    GetMetal(item, depth + 1, allowAluminum) * item.stackCount
                );
            combinedMass +=
                (pawn.apparel?.WornApparel ?? []).Sum(item => GetMetal(item, depth + 1, allowAluminum) * item.stackCount
                );
            combinedMass +=
                (pawn.equipment?.AllEquipmentListForReading ?? []).Sum(item =>
                    GetMetal(item, depth + 1) * item.stackCount
                );

            if (combinedMass > 0f) return combinedMass;
        }

        if (!thing.def.killedLeavings.NullOrEmpty()) {
            return thing.def.killedLeavings.Sum(def => GetMetalForThingDefCountClass(def, depth + 1, allowAluminum));
        }

        if (!thing.def.costList.NullOrEmpty()) {
            return thing.def.costList.Sum(def => GetMetalForThingDefCountClass(def, depth + 1, allowAluminum));
        }

        return 0f;
    }

    /// <summary>
    ///     This isn't entirely accurate at getting the metal mass of an item, but its a rough implementation.
    /// </summary>
    public static float GetMetal(Thing thing, int depth = 0, bool allowAluminum = false) {
        if (thing?.def == null) return 0f;
        if (!MetalThingCache.TryGetValue(thing, out float value)) {
            value = CalculateMetal(thing, depth, allowAluminum);
            MetalThingCache.Add(thing, value);
        }

        return value;
    }

    public static float GetMetalForThingDefCountClass(ThingDefCountClass def, int depth, bool allowAluminum = false) {
        Thing? item = ThingMaker.MakeThing(def.thingDef);
        float metalMass = GetMetal(item, depth + 1, allowAluminum);
        return metalMass * def.count;
    }

    public static List<RecipeDef> RecipesThatMake(ThingDef thingDef) {
        return DefDatabase<RecipeDef>.AllDefsListForReading
            .Where(recipe =>
                recipe.products != null &&
                recipe.products.Any(p => p.thingDef == thingDef)
            )
            .ToList();
    }

    public static bool RecipeUsesMetalIngredient(RecipeDef recipe, int depth, bool allowAluminum = false) {
        if (MetalRecipeCache.TryGetValue(recipe, out bool metalIngredient)) return metalIngredient;

        if (recipe.ingredients == null || recipe.ingredients.Count == 0) {
            MetalRecipeCache[recipe] = false;
            return false;
        }

        if (!Enumerable.Any(
                recipe.ingredients,
                ingredient => ingredient.filter.AllowedThingDefs.Any(thingDef =>
                    GetMetal(ThingMaker.MakeThing(thingDef), depth, allowAluminum) > 0f
                )
            )) {
            MetalRecipeCache[recipe] = false;
            return false;
        }

        MetalRecipeCache[recipe] = true;
        return true;
    }
}