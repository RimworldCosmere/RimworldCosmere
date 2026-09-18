using Cosmere.Core.Def;
using Cosmere.Core.DefModExtension;
using RimWorld;
using Verse;

namespace Cosmere.Core.Util;

public static class MetalDetector {
    private static readonly Dictionary<RecipeDef, bool> MetalRecipeCache = new Dictionary<RecipeDef, bool>();
    private static readonly Dictionary<Verse.Thing, float> MetalThingCache = new Dictionary<Verse.Thing, float>();

    // keyed by def, not by the throwaway sample Thing, or every recipe walk re-makes and re-walks every ingredient
    private static readonly Dictionary<(ThingDef, bool), float> SampleMassCache = new Dictionary<(ThingDef, bool), float>();
    private static readonly HashSet<(ThingDef, bool)> SamplesInProgress = new HashSet<(ThingDef, bool)>();

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

    public static bool HasMetal(Verse.Thing? thing, int depth = 0, bool allowAluminum = false) {
        return GetMetalMass(thing, depth, allowAluminum) > 0f;
    }

    private static float CalculateMetalMass(Verse.Thing? thing, int depth = 0, bool allowAluminum = false) {
        if (!IsCapableOfHavingMetal(thing?.def)) return 0f;
        if (thing?.def == null || depth > 25) return 0f;

        List<MetalDef> metals = GetLinkedMetals(thing.def, allowAluminum);
        if (metals.Count > 0 ||
            thing.def.IsMetal &&
            (allowAluminum || !thing.def.Equals(ThingDefOf.Aluminum))) {
            if (thing.def.category != ThingCategory.Building) {
                return thing.GetStatValue(RimWorld.StatDefOf.Mass);
            }

            if (thing.def.defName.StartsWith("Mineable")) {
                return thing.def.building.mineableYield *
                       10 *
                       metals.First().Item.GetStatValueAbstract(RimWorld.StatDefOf.Mass);
            }
        }

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
                    GetMetalMass(item, depth + 1, allowAluminum) * item.stackCount
                );
            combinedMass +=
                (pawn.apparel?.WornApparel ?? []).Sum(item => GetMetalMass(item, depth + 1, allowAluminum) * item.stackCount
                );
            combinedMass +=
                (pawn.equipment?.AllEquipmentListForReading ?? []).Sum(item =>
                    GetMetalMass(item, depth + 1) * item.stackCount
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
    /// <returns></returns>
    public static float GetMetalMass(Verse.Thing? thing, int depth = 0, bool allowAluminum = false) {
        if (thing?.def == null) return 0f;
        if (!MetalThingCache.TryGetValue(thing, out float value)) {
            value = CalculateMetalMass(thing, depth, allowAluminum);
            MetalThingCache.Add(thing, value);
        }

        return value;
    }

    /// <summary>
    ///     ThingMaker logs an error for any madeFromStuff def handed no stuff, and both callers below
    ///     walk arbitrary defs off cost lists and ingredient filters. Glass was the one that surfaced it.
    /// </summary>
    private static Verse.Thing MakeSample(ThingDef thingDef) {
        return ThingMaker.MakeThing(
            thingDef,
            thingDef.MadeFromStuff ? RimWorld.GenStuff.DefaultStuffFor(thingDef) : null
        );
    }

    public static float GetMetalForThingDefCountClass(ThingDefCountClass def, int depth, bool allowAluminum = false) {
        return SampleMass(def.thingDef, depth + 1, allowAluminum) * def.count;
    }

    /// <summary>
    ///     Metal mass of a default-stuff sample of the def. A def already mid-walk reads as 0, so a
    ///     recipe cycle (A needs B, B is made from A) stops instead of recursing.
    /// </summary>
    private static float SampleMass(ThingDef thingDef, int depth, bool allowAluminum) {
        (ThingDef, bool) key = (thingDef, allowAluminum);
        if (SampleMassCache.TryGetValue(key, out float cached)) return cached;
        if (!SamplesInProgress.Add(key)) return 0f;

        float mass = CalculateMetalMass(MakeSample(thingDef), depth, allowAluminum);
        SamplesInProgress.Remove(key);
        SampleMassCache[key] = mass;
        return mass;
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
                    SampleMass(thingDef, depth, allowAluminum) > 0f
                )
            )) {
            MetalRecipeCache[recipe] = false;
            return false;
        }

        MetalRecipeCache[recipe] = true;
        return true;
    }
}
