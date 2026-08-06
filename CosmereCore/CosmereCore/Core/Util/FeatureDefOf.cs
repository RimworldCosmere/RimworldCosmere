using Cosmere.Core.Def;
using RimWorld;
using Verse;

namespace Cosmere.Core.Util;

/// <summary>
///     The phenomena Core knows by name. Each is optional - the shard mod that declares it may
///     not be loaded, and a gate reading a null feature correctly answers "not happening".
/// </summary>
[DefOf]
public static class FeatureDefOf {
    [MayRequire("Cosmere.Roshar")]
    public static CosmereFeatureDef? Cosmere_Feature_Highstorms;

    [MayRequire("Cosmere.Roshar")]
    public static CosmereFeatureDef? Cosmere_Feature_StormlightNetwork;

    [MayRequire("Cosmere.Roshar")]
    public static CosmereFeatureDef? Cosmere_Feature_Spren;

    [MayRequire("Cosmere.Roshar")]
    public static CosmereFeatureDef? Cosmere_Feature_Nightwatcher;

    [MayRequire("Cosmere.Scadrial")]
    public static CosmereFeatureDef? Cosmere_Feature_Mists;

    [MayRequire("Cosmere.Scadrial")]
    public static CosmereFeatureDef? Cosmere_Feature_Ashfall;

    [MayRequire("Cosmere.Scadrial")]
    public static CosmereFeatureDef? Cosmere_Feature_PreservationBead;

    static FeatureDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(FeatureDefOf));
    }
}
