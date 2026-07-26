#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.Core.UI.Radial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class RadialKeyBindingDefOf {
    public static KeyBindingDef Cosmere_Keybind_RadialOpen;

    static RadialKeyBindingDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(RadialKeyBindingDefOf));
    }
}