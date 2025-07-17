#nullable disable
using System.Diagnostics.CodeAnalysis;
using CosmereCore.Def;
using RimWorld;

namespace CosmereRoshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ShardDefOf {
    public static ShardDef Honor;
    public static ShardDef Cultivation;
    public static ShardDef Odium;
    public static ShardDef Retribution;

    static ShardDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ShardDefOf));
    }
}