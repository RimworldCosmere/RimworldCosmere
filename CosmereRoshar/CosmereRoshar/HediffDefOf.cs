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
public class HediffDefOf {
    public static HediffDef Cosmere_Roshar_Hediff_ShardbladeSummoning;

    static HediffDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }
}