using Cosmere.Core.Lib.FloatSubMenu;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using Cosmere.System.Scadrial.Hemalurgy.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Hemalurgy.FloatMenuOptionProvider;

public class ImplantSpikeMenuProvider : HemalurgySpikeMenuProviderBase {
    protected override FloatMenuOption? GetSingleOptionFor(Verse.Thing clickedThing, FloatMenuContext context) {
        if (!ValidateSurgeryPair(clickedThing, context, out Pawn target, out Pawn surgeon)) return null;

        List<Verse.Thing> chargedSpikes = HemalurgySurgeryJob.FindSpikes(surgeon, true);
        if (chargedSpikes.Count == 0) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ImplantLiveSpike".Translate(target.LabelShortCap) +
                ": " +
                "CS_Hemalurgy_NoChargedSpike".Translate(),
                null
            );
        }

        if (!surgeon.CanReach(target, PathEndMode.ClosestTouch, Danger.Deadly)) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ImplantLiveSpike".Translate(target.LabelShortCap) +
                ": " +
                "NoPath".Translate().CapitalizeFirst(),
                null
            );
        }

        List<FloatMenuOption> subOptions = BuildSpikeOptions(surgeon, target, chargedSpikes);
        if (subOptions.Count == 0) return null;

        return FloatSubMenuFactory.CompatMMMCreate(
            "CS_Hemalurgy_ImplantLiveSpike".Translate(target.LabelShortCap),
            subOptions
        );
    }

    private List<FloatMenuOption> BuildSpikeOptions(Pawn surgeon, Pawn target, List<Verse.Thing> spikes) {
        List<FloatMenuOption> options = [];
        for (int i = 0; i < spikes.Count; i++) {
            Verse.Thing spike = spikes[i];
            HemalurgicSpike? comp = spike.TryGetComp<HemalurgicSpike>();
            if (comp == null) continue;

            HemalurgicStealType stealType = comp.chargeData!.stealType;
            if (stealType == HemalurgicStealType.RemoveAllPowers) {
                options.Add(
                    new FloatMenuOption(
                        HemalurgicSpikeLabels.GetSpikeLabel(spike, comp, stealType) + ": " + "CS_Hemalurgy_CannotImplantAluminum".Translate(),
                        null
                    )
                );
                continue;
            }

            string label = HemalurgicSpikeLabels.GetSpikeLabel(spike, comp, stealType);

            options.Add(
                new FloatMenuOption(
                    label,
                    () => HemalurgySurgeryJob.TryStart(
                        surgeon,
                        target,
                        spike,
                        JobDefOf.Cosmere_Scadrial_Job_ImplantSpike
                    )
                )
            );
        }

        return options;
    }
}
