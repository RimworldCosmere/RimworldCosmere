#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

/// <summary>
///     Two faces per build. Which one a koloss wears is fixed for its life; which pair it picks
///     from moves when the body does.
/// </summary>
[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public static class HeadTypeDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static HeadTypeDef Cosmere_Scadrial_KolossHead_Young_A;

    [MayRequire("Cosmere.Scadrial")]
    public static HeadTypeDef Cosmere_Scadrial_KolossHead_Young_B;

    [MayRequire("Cosmere.Scadrial")]
    public static HeadTypeDef Cosmere_Scadrial_KolossHead_Mature_A;

    [MayRequire("Cosmere.Scadrial")]
    public static HeadTypeDef Cosmere_Scadrial_KolossHead_Mature_B;

    static HeadTypeDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(HeadTypeDefOf));
    }
}
