using System.Collections.Generic;
using Cosmere.Core.Def;
using Cosmere.Core.DefModExtension;
using RimWorld;
using Verse;
using DefModExtension_Shards = Cosmere.Core.DefModExtension.Shards;

namespace Cosmere.Core.Util;

/// <summary>
///     Finding the ScenarioDef behind the running Scenario, and reading what it declares.
/// </summary>
/// <remarks>
///     Three separate patches used to repeat this lookup inline. It matches on label because
///     Scenario carries no back-reference to its def, and ScenarioDef.PostLoad copies label into
///     scenario.name when the XML omits an inner name - which every scenario we ship does. A
///     player-saved custom scenario resolves to null, and every caller has to cope with that.
/// </remarks>
public static class ScenarioDefUtility {
    public static ScenarioDef? Current {
        get {
            string? name = Find.Scenario?.name;
            if (string.IsNullOrEmpty(name)) return null;

            List<ScenarioDef> defs = DefDatabase<ScenarioDef>.AllDefsListForReading;
            for (int i = 0; i < defs.Count; i++) {
                if (defs[i].label == name) return defs[i];
            }

            return null;
        }
    }

    /// <summary>
    ///     Whether the running scenario is one we ship, by the mod that declares it.
    /// </summary>
    /// <remarks>
    ///     This used to be a set of label prefixes - StartsWith("Stormlight:"), ("Roshar:"),
    ///     ("Cosmere:") - and not one of them matched a shipped label, which is
    ///     "Cosmere - Scadrial - The Final Empire". Only Scadrial filtered, and only by the
    ///     accident of a Contains("Scadrial") sitting alongside them. Asking the mod is not
    ///     guessable wrong.
    /// </remarks>
    public static bool IsCosmere {
        get {
            string? packageId = Current?.modContentPack?.PackageId;
            return packageId != null &&
                packageId.StartsWith("cosmere.", global::System.StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>The era the running scenario starts in, or null when it names none.</summary>
    public static EraDef? CurrentEra => Current?.GetModExtension<ScenarioEra>()?.era;

    /// <summary>What the running scenario says about Shards, or null when it says nothing.</summary>
    public static DefModExtension_Shards? CurrentShards => Current?.GetModExtension<DefModExtension_Shards>();

    /// <summary>
    ///     Whether the player may change the world and Shards. True when no scenario is
    ///     recognised, so a custom scenario stays editable rather than silently locking.
    /// </summary>
    public static bool AllowsChange => CurrentShards?.allowChange ?? true;
}
