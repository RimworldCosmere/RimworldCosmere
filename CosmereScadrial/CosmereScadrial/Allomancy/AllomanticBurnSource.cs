#nullable disable
using System;
using CosmereScadrial.Def;
using Verse;

namespace CosmereScadrial.Allomancy;

public struct AllomanticBurnSource : IExposable, IEquatable<AllomanticBurnSource> {
    public AllomanticAbilityDef Def;
    public float Rate;

    public AllomanticBurnSource() { }

    public AllomanticBurnSource(AllomanticAbilityDef def, float rate) {
        Def = def;
        Rate = rate;
    }

    public void ExposeData() {
        Scribe_Defs.Look(ref Def, "def");
        Scribe_Values.Look(ref Rate, "rate");
    }

    public bool Equals(AllomanticBurnSource other) {
        return Def.Equals(other.Def);
    }

    public override bool Equals(object obj) {
        return obj is AllomanticBurnSource other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Def, Rate);
    }

    public static implicit operator (AllomanticAbilityDef def, float rate)(AllomanticBurnSource source) {
        return (source.Def, source.Rate);
    }

    public static implicit operator AllomanticBurnSource((AllomanticAbilityDef def, float rate) tuple) {
        return new AllomanticBurnSource { Def = tuple.def, Rate = tuple.rate };
    }
}