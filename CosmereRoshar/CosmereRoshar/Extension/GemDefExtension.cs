using Cosmere.Resources.Def;
using Cosmere.Roshar.Def;
using Verse;

namespace Cosmere.Roshar.Extension;

public static class GemDefExtension {
    public static GeneDef? GetSurgebindingGene(this GemDef gemDef) {
        RadiantOrderDef? order =
            DefDatabase<RadiantOrderDef>.AllDefsListForReading.FirstOrDefault(x => x.gemstone.Equals(gemDef));

        return order?.GetSurgebindingGene();
    }
}