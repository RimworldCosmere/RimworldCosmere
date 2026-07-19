#nullable disable
using System.Diagnostics.CodeAnalysis;
using Cosmere.Core.Def;
using RimWorld;

namespace Cosmere.System.Nalthis;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ShardDefOf {
    [MayRequire("Cosmere.Nalthis")]
    public static ShardDef Endowment;

    static ShardDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ShardDefOf));
    }
}
