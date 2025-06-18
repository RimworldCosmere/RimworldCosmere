using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class AbilityDefOf {
    public static AbilityDef Cosmere_Ability_Brass_Aura;
    public static AbilityDef Cosmere_Ability_Brass_Target;
    public static AbilityDef Cosmere_Ability_Bronze_Aura;
    public static AbilityDef Cosmere_Ability_Copper_Aura;
    public static AbilityDef Cosmere_Ability_Aluminum;
    public static AbilityDef Cosmere_Ability_Cadmium;
    public static AbilityDef Cosmere_Ability_Electrum;
    public static AbilityDef Cosmere_Ability_Atium;
    public static AbilityDef Cosmere_Ability_Bendalloy;
    public static AbilityDef Cosmere_Ability_Duralumin_Surge;
    public static AbilityDef Cosmere_Ability_Nicrosil_Surge;
    public static AbilityDef Cosmere_Ability_Iron_Aura;
    public static AbilityDef Cosmere_Ability_Iron_Pull;
    public static AbilityDef Cosmere_Ability_Pewter;
    public static AbilityDef Cosmere_Ability_Steel_Aura;
    public static AbilityDef Cosmere_Ability_Steel_Push;
    public static AbilityDef Cosmere_Ability_Steel_Coinshot;
    public static AbilityDef Cosmere_Ability_Tin;
    public static AbilityDef Cosmere_Ability_Zinc_Aura;
    public static AbilityDef Cosmere_Ability_Zinc_Target;

    static AbilityDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
    }
}