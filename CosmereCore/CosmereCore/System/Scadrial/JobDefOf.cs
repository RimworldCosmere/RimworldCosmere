#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class JobDefOf {
    static JobDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
    }

    [MayRequire("Cosmere.Scadrial")]
    public static JobDef Cosmere_Scadrial_Job_MaintainAllomanticTarget;

    [MayRequire("Cosmere.Scadrial")]
    public static JobDef Cosmere_Scadrial_Job_CastAllomanticAbilityAtTarget;

    [MayRequire("Cosmere.Scadrial")]
    public static JobDef Cosmere_Scadrial_Job_FollowGoldHallucination;

    [MayRequire("Cosmere.Scadrial")]
    public static JobDef Cosmere_Scadrial_Job_GivePatientVial;

    [MayRequire("Cosmere.Scadrial")]
    public static JobDef Cosmere_Scadrial_Job_ChargeCorpseSpike;

    [MayRequire("Cosmere.Scadrial")]
    public static JobDef Cosmere_Scadrial_Job_ChargeLiveSpike;

    [MayRequire("Cosmere.Scadrial")]
    public static JobDef Cosmere_Scadrial_Job_WaitInBed;

    [MayRequire("Cosmere.Scadrial")]
    public static JobDef Cosmere_Scadrial_Job_ImplantSpike;
}