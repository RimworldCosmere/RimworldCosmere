using Cosmere.Core.Def;
using RimWorld;
using Verse;

namespace Cosmere.Core.Util;

/// <summary>
///     The worlds Core knows by name. Each is optional - a shard mod may not be loaded, in which
///     case the field stays null and every gate reading it correctly answers "not here".
/// </summary>
[DefOf]
public static class WorldDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static CosmereWorldDef? Scadrial;

    [MayRequire("Cosmere.Roshar")]
    public static CosmereWorldDef? Roshar;

    /// <summary>The cross-world sentinel. Ships from Core, so it is always present.</summary>
    public static CosmereWorldDef? UnnamedPlanet;

    static WorldDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(WorldDefOf));
    }
}
