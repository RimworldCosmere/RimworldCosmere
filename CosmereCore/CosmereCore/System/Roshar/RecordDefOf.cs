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
    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_PatientsSaved;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_EnemyPatientsSaved;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_BondsFormed;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_PatientsDied;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_PawnsRescued;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_HostilePawnsRescued;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_ArrestsMade;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_ZoneComplianceDays;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_JudgmentsPassed;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_FuryMastered;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_CleanRaids;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_ForgottenTended;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_DeadHonored;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_GraveVisits;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_SocialHealing;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_LivesSaved;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_ResearchCompleted;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_ChallengesSurvived;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_PrisonersFreed;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_CaravanTrips;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_StructuresBuilt;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_RaidsDefended;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_LastStanding;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_FriendshipsFormed;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_ConflictsResolved;

    [MayRequire("Cosmere.Roshar")]
    public static RecordDef Cosmere_Roshar_Record_ColonyStabilityDays;

    static RecordDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(RecordDefOf));
    }
}
