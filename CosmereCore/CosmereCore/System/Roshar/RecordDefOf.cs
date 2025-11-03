#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class RecordDefOf {
    static RecordDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(RecordDefOf));
    }

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_PatientsSaved;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_EnemyPatientsSaved;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_BondsFormed;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_PatientsDied;
}