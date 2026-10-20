#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class SkillDefOf {
    [MayRequire("Cosmere.Roshar")]
    public static SkillDef Cosmere_Roshar_Skill_SurgebindingPower;

    static SkillDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(SkillDefOf));
    }
}
