#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class RecordDefOf {
    public static RecordDef Cosmere_Roshar_Record_PatientsSaved;
    public static RecordDef Cosmere_Roshar_Record_EnemyPatientsSaved;
    public static RecordDef Cosmere_Roshar_Record_BondsFormed;
    public static RecordDef Cosmere_Roshar_Record_PatientsDied;

    static RecordDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(RecordDefOf));
    }
}