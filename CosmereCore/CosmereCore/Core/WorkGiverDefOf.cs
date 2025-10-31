#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.Core;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class WorkGiverDefOf {
    public static WorkGiverDef Cosmere_StoreInApparelInnerStorage;

    static WorkGiverDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(WorkGiverDefOf));
    }
}