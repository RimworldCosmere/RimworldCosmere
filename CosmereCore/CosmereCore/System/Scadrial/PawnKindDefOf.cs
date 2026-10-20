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
public static class PawnKindDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static PawnKindDef Cosmere_Scadrial_PawnKind_GoldShadow;

    static PawnKindDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
    }
}
