using System.Linq;
using System.Text;
using CosmereScadrial.Comps.Things;
using Verse;

namespace CosmereScadrial.Abilities.Hediffs {
    public class BurningMetals : Hediff {
        public override string GetTooltip(Pawn pawn, bool showHediffsDebugInfo) {
            var comp = pawn.GetComp<MetalBurning>();
            var sb = new StringBuilder();

            sb.AppendLineTagged((TaggedString)LabelCap.Colorize(ColoredText.TipSectionTitleColor));

            var sources = comp.burnSources.Where(x => x.Value.Count > 0).ToList();
            if (!sources.Any()) {
                sb.AppendLine().AppendLine("Not burning or flaring any metals.");

                return sb.ToString();
            }

            sb.AppendLine().AppendLine(Description);
            foreach (var (metal, rateList) in sources) {
                var sum = rateList.Sum();
                if (sum > 0) sb.AppendLine($"{metal.LabelCap}: {sum * GenTicks.TicksPerRealSecond:0.00000} BEUs/second ({rateList.Count} sources)");
            }

            return sb.ToString();
        }
    }
}