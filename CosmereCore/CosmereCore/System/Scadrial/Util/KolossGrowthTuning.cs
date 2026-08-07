using System;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Rewrites the koloss growth rate from the mod setting.
/// </summary>
/// <remarks>
///     The hediff def carries eight years as its default, which is what the XML should say - a
///     reader opening the file deserves the real number rather than a placeholder. The setting
///     then rewrites it, at startup and again whenever the settings window closes, so a change
///     takes hold without a restart.
///     <para>
///         Mutating a shared def is global by nature, which is exactly what a mod setting is.
///         The value is recomputed from the setting every time rather than adjusted, so applying
///         it twice is the same as applying it once.
///     </para>
/// </remarks>
[StaticConstructorOnStartup]
public static class KolossGrowthTuning {
    /// <summary>What the XML says, and what the setting resets to.</summary>
    public const float DefaultYears = 8f;

    public const float MinYears = 1f;
    public const float MaxYears = 30f;

    static KolossGrowthTuning() {
        Apply();
    }

    public static void Apply() {
        HediffDef? growth = HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth;
        if (growth?.comps == null) return;

        float years = Mod.kolossGrowthYears;
        if (years < MinYears) years = MinYears;

        for (int i = 0; i < growth.comps.Count; i++) {
            if (growth.comps[i] is not HediffCompProperties_SeverityPerDay perDay) continue;

            // Severity runs 0 to 1 over the whole life, so the daily step is one over the number
            // of days in that span.
            perDay.severityPerDay = 1f / (years * GenDate.DaysPerYear);

            Cosmere.Core.Logger.Verbose(
                $"KolossGrowthTuning: growth set to {years:0.#} years " +
                $"({perDay.severityPerDay:0.000000} severity/day)."
            );

            return;
        }
    }
}
