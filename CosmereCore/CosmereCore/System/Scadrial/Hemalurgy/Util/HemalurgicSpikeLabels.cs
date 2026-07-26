using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.Util;

public static class HemalurgicSpikeLabels {
    public static string GetSpikeLabel(Verse.Thing spike, HemalurgicSpike comp, HemalurgicStealType stealType) {
        bool isNeedle = spike.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;
        string label = isNeedle ? "CS_Hemalurgy_NeedleOption" : "CS_Hemalurgy_SpikeOption";
        string metalLabel = comp.metal?.LabelCap ?? "unknown";
        string stealLabel = HemalurgicConstants.GetStealTypeLabel(stealType);
        return label.Translate(metalLabel, stealLabel);
    }
}
