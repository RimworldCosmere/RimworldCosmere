using Cosmere;
using Cosmere.Def;
using Cosmere.System.Roshar.Def;
using Verse;

namespace Cosmere.System.Roshar.Extension;

public static class GemDefExtension {
    public static GeneDef? GetSurgebindingGene(this GemDef gemDef) {
        RadiantOrderDef? order =
            DefDatabase<RadiantOrderDef>.AllDefsListForReading.FirstOrDefault(x => x.gemstone.Equals(gemDef));

        return order?.GetSurgebindingGene();
    }
}