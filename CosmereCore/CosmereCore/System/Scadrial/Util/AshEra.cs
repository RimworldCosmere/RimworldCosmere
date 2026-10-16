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
    /// <returns>True when that era is the one ash falls in.</returns>
    public static bool IsAshEra(string? eraDefName) {
        return eraDefName == AshEraDefName;
    }

    /// <summary>Governs map generation, the ashfall condition, eruptions and deposition.</summary>
    /// <returns>True when ash should still be falling on this map.</returns>
    public static bool CanAccumulate(Verse.Map? map) {
        if (map == null) return false;
        if (!Mod.EnableAshfall) return false;

        // world/era gated, not Shard - the Ashmounts are Rashek's engineering, run regardless of which Shards are held.
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Ashfall)) return false;

        return IsAshEra(CosmereQuestManager.FindActiveEra());
    }

    /// <summary>
    ///     Governs the section layer and the falling veil. Outlives the era flip so ash already on
    ///     the ground drains visibly rather than disappearing the moment Harmony rises.
    /// </summary>
    /// <returns>True when ash should be drawn, either because it is still falling or because some is left on the ground.</returns>
    public static bool ShouldRender(Verse.Map? map) {
        if (map == null) return false;
        if (CanAccumulate(map)) return true;

        return map.GetComponent<AshDepthTracker>()?.HasAnyAsh == true;
    }
}
