using System.Collections.Generic;
using System.Linq;
using System.Text;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs;

public class BurningMetals : Hediff {
    public override string GetTooltip(Pawn pawn, bool showHediffsDebugInfo) {
        MetalBurning? comp = pawn.GetComp<MetalBurning>();
        StringBuilder sb = new StringBuilder();

        sb.AppendLineTagged((TaggedString)LabelCap.Colorize(ColoredText.TipSectionTitleColor));

        List<KeyValuePair<MetallicArtsMetalDef, List<float>>> sources =
            comp.burnSources.Where(x => x.Value.Count > 0).ToList();
        if (!sources.Any()) {
            sb.AppendLine().AppendLine("Not burning or flaring any metals.");

            return sb.ToString();
        }

        sb.AppendLine().AppendLine(Description);
        foreach ((MetallicArtsMetalDef? metal, List<float>? rateList) in sources) {
            float sum = rateList.Sum();
            if (sum > 0) {
                sb.AppendLine(
                    $"{metal.LabelCap}: {sum * GenTicks.TicksPerRealSecond:0.00000} BEUs/second ({rateList.Count} sources)");
            }
        }

        return sb.ToString();
    }
}