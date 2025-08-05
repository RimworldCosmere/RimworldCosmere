#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class LetterDefOf {
    public static LetterDef Cosmere_Roshar_ChooseRadiantOrder;

    static LetterDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(LetterDefOf));
    }
}