#nullable disable
using System.Diagnostics.CodeAnalysis;
using Cosmere.Core.Def;
using RimWorld;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ShardDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static ShardDef Ruin;

    [MayRequire("Cosmere.Scadrial")]
    public static ShardDef Preservation;

    [MayRequire("Cosmere.Scadrial")]
    public static ShardDef Harmony;

    static ShardDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ShardDefOf));
    }
}
