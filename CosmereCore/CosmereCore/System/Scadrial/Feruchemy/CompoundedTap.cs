using System;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy;

/// Compounded charge taps through its own hediff def, whose stage ladder already
/// carries the amplification for anything declared in XML. Metals that implement
/// their effect in C# instead have to recognise that def as tapping and scale
/// themselves, so the multiplier lives here rather than in each of them.
public static class CompoundedTap {
    public const float EffectMultiplier = 10f;

    private const string Prefix = "Cosmere_Scadrial_Hediff_TapCompounded";

    public static bool IsCompounded(Verse.Def def) {
        return def.defName.StartsWith(Prefix, StringComparison.Ordinal);
    }

    /// True for either the ordinary tap def or its compounded counterpart.
    public static bool IsTap(Verse.Def def, Verse.Def? ordinary) {
        return (ordinary != null && def.Equals(ordinary)) || IsCompounded(def);
    }

    public static float Scale(Verse.Def def, float value) {
        return IsCompounded(def) ? value * EffectMultiplier : value;
    }
}
