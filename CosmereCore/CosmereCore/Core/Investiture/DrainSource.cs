using System;
using Cosmere.Core.Def;
using Verse;

namespace Cosmere.Core.Investiture;

public struct DrainSource : IExposable, IEquatable<DrainSource> {
    public AbilityDef Def = null!;
    public float Rate;

    public DrainSource() { }

    public DrainSource(AbilityDef def, float rate) {
        Def = def;
        Rate = rate;
    }

    public void ExposeData() {
        Scribe_Defs.Look(ref Def, "def");
        Scribe_Values.Look(ref Rate, "rate");
    }

    public bool Equals(DrainSource other) {
        return Def.Equals(other.Def);
    }

    public override bool Equals(object obj) {
        return obj is DrainSource other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Def, Rate);
    }

    public static implicit operator (AbilityDef def, float rate)(DrainSource source) {
        return (source.Def, source.Rate);
    }

    public static implicit operator DrainSource((AbilityDef def, float rate) tuple) {
        return new DrainSource { Def = tuple.def, Rate = tuple.rate };
    }
}