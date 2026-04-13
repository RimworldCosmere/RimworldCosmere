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
public static class LetterDefOf {
    static LetterDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(LetterDefOf));
    }

    [MayRequire("Cosmere.Roshar")]
    public static LetterDef Cosmere_Roshar_ChooseRadiantOrder;

    [MayRequire("Cosmere.Roshar")]
    public static LetterDef Cosmere_Roshar_Letter_SpeakOath;
}