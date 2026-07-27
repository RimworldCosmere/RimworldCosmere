#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ThoughtDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Scadrial_Snapped;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Copper_Clouded;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Dread;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Awe;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Gratitude;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Regret;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Shame;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Rage;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Loss;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Hope;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Pride;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Determination;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Jealousy;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_Gold_Curiosity;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Thought_PostGold_Afterglow;

    static ThoughtDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
    }
}
