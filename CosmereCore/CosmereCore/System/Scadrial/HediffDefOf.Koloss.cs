#nullable disable
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

public static partial class HediffDefOf {
    /// <summary>
    ///     Severity is the koloss's size. The render worker reads it every frame, and the last
    ///     stage is where the skin gives out for good.
    /// </summary>
    [MayRequire("Cosmere.Scadrial")]
    public static HediffDef Cosmere_Scadrial_Hediff_KolossGrowth;
}
