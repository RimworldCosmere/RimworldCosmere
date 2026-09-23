#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.Core;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class SoundDefOf {
    public static SoundDef Cosmere_Core_Sound_LoadingQuantumRiser;

    static SoundDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(SoundDefOf));
    }
}
