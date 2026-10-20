using Verse;

namespace Cosmere.System.Scadrial.Def;

/// <summary>
///     How far along its growth a koloss of this kind arrives.
/// </summary>
/// <remarks>
///     A koloss made by surgery starts at nothing, because the clock starts at the spikes. One
///     that marches out of the east has been somebody's for years, and a raid of newborns would
///     read as a raid of large men. PawnKindDef has no hook for this and the growth hediff's
///     initialSeverity is one number for every koloss alive, so the kind carries the range and
///     the gene reads it when it seeds the clock.
/// </remarks>
public class KolossGrowthExtension : DefModExtension {
    /// <summary>Severity range this kind is generated somewhere inside.</summary>
    public FloatRange growth = new(0.001f, 0.001f);
}
