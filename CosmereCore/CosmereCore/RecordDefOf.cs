#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereCore;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class RecordDefOf {
    public static RecordDef Cosmere_InvestitureGained;
    public static RecordDef Cosmere_InvestitureSpent;

    static RecordDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(RecordDefOf));
    }
}