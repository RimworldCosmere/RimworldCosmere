#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class SkillDefOf {
    public static SkillDef Cosmere_Scadrial_Skill_AllomanticPower;
    public static SkillDef Cosmere_Scadrial_Skill_FeruchemicPower;

    static SkillDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(SkillDefOf));
    }
}