#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereCore;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class StatDefOf {
    public static StatDef Cosmere_Mental_Break_Add_Factor;
    public static StatDef Cosmere_Mental_Break_Remove_Factor;
    public static StatDef Cosmere_Time_Dilation_Factor;
    public static StatDef Cosmere_Investiture;

    static StatDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf));
    }
}