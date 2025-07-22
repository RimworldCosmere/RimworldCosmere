using Cosmere.Resources.Def;
using RimWorld;
using Verse;

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

    public GeneDef GetSurgebindingGene() {
        return DefDatabase<GeneDef>.GetNamed("Cosmere_Roshar_Gene_Radiant" + defName);
    }
}