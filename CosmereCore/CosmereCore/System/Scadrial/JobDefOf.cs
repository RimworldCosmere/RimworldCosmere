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
    public static JobDef Cosmere_Scadrial_Job_MaintainAllomanticTarget;
    public static JobDef Cosmere_Scadrial_Job_CastAllomanticAbilityAtTarget;
    public static JobDef Cosmere_Scadrial_Job_FollowGoldHallucination;

    public static JobDef Cosmere_Scadrial_Job_GivePatientVial;
    //public static JobDef Cosmere_Scadrial_Job_EquipMetalmind;
    //public static JobDef Cosmere_Scadrial_Job_UnequipMetalmind;

    static JobDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
    }
}