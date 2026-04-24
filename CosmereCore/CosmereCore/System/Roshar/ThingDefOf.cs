#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ThingDefOf {
    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_Chip;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_Mark;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_Broam;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Apparel_SpherePouch;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_SphereLampWall;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_Highstorm;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Race_Spren;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Race_UnknownTrueSpren;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_MeleeWeapon_RadiantShardblade;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_MeleeWeapon_DeadShardblade;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Apparel_RadiantShardplate;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Apparel_RadiantShardhelm;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Mote_OathBurst;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_Gemheart;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_SurgePortal;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_WindsprenShieldAura;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_EntropicAura;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_BloomAura;

    [MayRequire("Cosmere.Roshar")]
    public static ThingDef Cosmere_Roshar_Thing_LifesurgePulse;

    static ThingDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}