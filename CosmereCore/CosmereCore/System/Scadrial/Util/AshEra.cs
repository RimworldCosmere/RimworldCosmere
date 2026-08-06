using Cosmere.Core.Quest;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Settings;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Decides which maps ash touches. Split in two on purpose: the Catacendre beat advances the
///     era and swaps Ruin and Preservation for Harmony in the same instant, so a single predicate
///     would make metres of ash vanish in one frame instead of draining over days.
/// </summary>
public static class AshEra {
    public const string AshEraDefName = "Cosmere_Scadrial_Era_PreCatacendre";

    /// <summary>Pure so it can be tested without Verse. Every era but the first is ash-free.</summary>
    public static bool IsAshEra(string? eraDefName) {
        return eraDefName == AshEraDefName;
    }

    /// <summary>Governs map generation, the ashfall condition, eruptions and deposition.</summary>
    public static bool CanAccumulate(Verse.Map? map) {
        if (map == null) return false;
        if (!Mod.enableAshfall) return false;

        // World and era, deliberately not Shard. The Ashmounts are Rashek's engineering, not a
        // Shard's doing - the ash falls because of what he built and stops when the Catacendre
        // undoes it, whichever Shards happen to be held.
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Ashfall)) return false;

        return IsAshEra(CosmereQuestManager.FindActiveEra());
    }

    /// <summary>
    ///     Governs the section layer and the falling veil. Outlives the era flip so ash already on
    ///     the ground drains visibly rather than disappearing the moment Harmony rises.
    /// </summary>
    public static bool ShouldRender(Verse.Map? map) {
        if (map == null) return false;
        if (CanAccumulate(map)) return true;

        return map.GetComponent<AshDepthTracker>()?.HasAnyAsh == true;
    }
}
