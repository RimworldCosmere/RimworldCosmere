#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class MentalStateDefOf {
    /// <summary>
    ///     A koloss nobody is holding. Aggro category, which is what sends it at the nearest thing.
    /// </summary>
    [MayRequire("Cosmere.Scadrial")]
    public static MentalStateDef Cosmere_Scadrial_MentalState_KolossBloodlust;
}
