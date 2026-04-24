#nullable disable
using System.Diagnostics.CodeAnalysis;
using Cosmere.Core.Def;
using RimWorld;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ShardDefOf {
    [MayRequire("Cosmere.Roshar")]
    public static ShardDef Honor;

    [MayRequire("Cosmere.Roshar")]
    public static ShardDef Cultivation;

    [MayRequire("Cosmere.Roshar")]
    public static ShardDef Odium;

    [MayRequire("Cosmere.Roshar")]
    public static ShardDef Retribution;

    static ShardDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ShardDefOf));
    }
}