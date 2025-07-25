#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ThingDefOf {
    public static ThingDef Cosmere_Roshar_Thing_Chip;
    public static ThingDef Cosmere_Roshar_Thing_Mark;
    public static ThingDef Cosmere_Roshar_Thing_Broam;
    public static ThingDef Cosmere_Roshar_Apparel_SpherePouch;
    public static ThingDef Cosmere_Roshar_Thing_SphereLampWall;
    public static ThingDef Cosmere_Roshar_Thing_Highstorm;

    static ThingDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}