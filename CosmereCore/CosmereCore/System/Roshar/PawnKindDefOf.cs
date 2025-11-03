#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class PawnKindDefOf {
    static PawnKindDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
    }

    [MayRequire("Cosmere.Roshar")]
    public static PawnKindDef Cosmere_Roshar_Race_Spren;

    [MayRequire("Cosmere.Roshar")]
    public static PawnKindDef Cosmere_Roshar_Race_UnknownTrueSpren;
}