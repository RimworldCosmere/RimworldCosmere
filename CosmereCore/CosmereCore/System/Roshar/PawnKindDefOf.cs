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
    public static PawnKindDef Cosmere_Roshar_Race_Spren;
    public static PawnKindDef Cosmere_Roshar_Race_UnknownTrueSpren;

    static PawnKindDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
    }
}