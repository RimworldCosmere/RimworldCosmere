using System.Collections.Generic;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using Verse;

namespace Cosmere.Core.Util;

/// <summary>
///     Whether a named phenomenon should be happening.
/// </summary>
/// <remarks>
///     Two questions in one place. On a normal world it is "am I on that world, and does a Shard
///     that causes this still exist" - highstorms need Roshar and Honor, both. On the cross-world
///     save there is no single world to ask about, so the player says which phenomena they want
///     and only the Shard requirement remains.
/// </remarks>
public static class FeatureUtility {
    public static bool IsActive(CosmereFeatureDef? feature) {
        if (feature == null) return false;

        if (feature.anyOfShards.Count > 0 && !ShardUtility.AreAnyEnabled(feature.anyOfShards.ToArray())) {
            return false;
        }

        CosmereWorldDef? current = WorldUtility.Primary;

        // no world chosen yet (early load, or a save predating world selection): fall back to Shard.
        if (current == null) return true;

        if (current.crossWorld) return IsChosen(feature);

        return feature.world == null || feature.world == current;
    }

    /// <summary>Whether the player has this switched on, defaulting to the def's own answer.</summary>
    public static bool IsChosen(CosmereFeatureDef feature) {
        CosmereWorld? comp = WorldUtility.component;
        if (comp == null) return feature.enabledByDefault;

        return comp.featureOverrides.TryGetValue(feature.defName, out bool chosen)
            ? chosen
            : feature.enabledByDefault;
    }

    public static void Set(CosmereFeatureDef feature, bool enabled) {
        CosmereWorld? comp = WorldUtility.component;
        if (comp == null) return;

        comp.featureOverrides[feature.defName] = enabled;
    }

    /// <summary>Every feature, in author-declared order.</summary>
    public static List<CosmereFeatureDef> All {
        get {
            List<CosmereFeatureDef> features = [..DefDatabase<CosmereFeatureDef>.AllDefsListForReading];
            features.SortBy(f => f.listOrder, f => f.defName);
            return features;
        }
    }
}
