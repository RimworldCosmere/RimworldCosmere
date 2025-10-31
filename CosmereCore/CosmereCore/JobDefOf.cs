#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class JobDefOf {
    public static JobDef Cosmere_HaulToInnerStorage;
    public static JobDef Cosmere_StoreInApparelInnerStorage;
    public static JobDef Cosmere_BondToThing;

    static JobDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
    }
}