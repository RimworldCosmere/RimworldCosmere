using System.Collections.Generic;
using Cosmere.Resources.Def;
using RimWorld;

namespace Cosmere.Roshar.Def;

public class Ideal {
    public List<AbilityDef> abilities;
    public string label;
    public List<string> quotes;
}

public class RadiantOrderDef : Verse.Def {
    public List<AbilityDef> abilities;
    public GemDef gemstone;
    public List<Ideal> ideals;
    public List<SurgeDef> surges;
}