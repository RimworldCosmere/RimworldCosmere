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
public static class JobDefOf {
    [MayRequire("Cosmere.Roshar")]
    public static JobDef Cosmere_Roshar_RefuelFabrial;

    [MayRequire("Cosmere.Roshar")]
    public static JobDef Cosmere_Roshar_RemoveFromFabrial;

    [MayRequire("Cosmere.Roshar")]
    public static JobDef Cosmere_Roshar_CaptureSpren;

    [MayRequire("Cosmere.Roshar")]
    public static JobDef Cosmere_Roshar_GoToHighstormShelter;

    [MayRequire("Cosmere.Roshar")]
    public static JobDef Cosmere_Roshar_WaitInHighstormShelter;

    [MayRequire("Cosmere.Roshar")]
    public static JobDef Cosmere_Roshar_Job_Soulcast;

    [MayRequire("Cosmere.Roshar")]
    public static JobDef Cosmere_Roshar_Job_ShapeStone;

    static JobDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
    }
}
