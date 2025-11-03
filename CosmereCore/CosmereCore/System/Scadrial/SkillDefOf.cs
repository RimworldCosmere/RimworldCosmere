#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class SkillDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static SkillDef Cosmere_Scadrial_Skill_AllomanticPower;

    [MayRequire("Cosmere.Scadrial")]
    public static SkillDef Cosmere_Scadrial_Skill_FeruchemicPower;
}