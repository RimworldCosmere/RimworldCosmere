#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.Foundation;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class JobDefOf {
    public static JobDef Cosmere_HaulToInnerStorage;
    public static JobDef Cosmere_StoreInApparelInnerStorage;

    static JobDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
    }
}