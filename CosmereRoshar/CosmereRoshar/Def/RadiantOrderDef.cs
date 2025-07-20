using System.Collections.Generic;
using Cosmere.Resources.Def;
using RimWorld;

namespace Cosmere.Roshar.Def;

public record Ideal {
    public List<AbilityDef> abilities;
    public string label;
    public List<string> quotes;
}

public class RadiantOrderDef : Verse.Def {
    public GemDef gemstone;
    public List<Ideal> ideals;
    public List<SurgeDef> surges;
}