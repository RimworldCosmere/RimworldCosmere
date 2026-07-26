using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Util;
using Cosmere.Core.Lib.FloatSubMenu;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Hemalurgy.FloatMenuOptionProvider;

public class LiveSpikeMenuProvider : HemalurgySpikeMenuProviderBase {
    protected override FloatMenuOption? GetSingleOptionFor(Verse.Thing clickedThing, FloatMenuContext context) {
        if (!ValidateSurgeryPair(clickedThing, context, out Pawn target, out Pawn surgeon)) return null;

        List<Verse.Thing> availableSpikes = HemalurgySurgeryJob.FindSpikes(surgeon, false);
        if (availableSpikes.Count == 0) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ChargeLiveSpike".Translate(target.LabelShortCap) +
                ": " +
                "CS_Hemalurgy_NoUnchargedSpike".Translate(),
                null
            );
        }

        if (!surgeon.CanReach(target, PathEndMode.ClosestTouch, Danger.Deadly)) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ChargeLiveSpike".Translate(target.LabelShortCap) +
                ": " +
                "NoPath".Translate().CapitalizeFirst(),
                null
            );
        }

        List<FloatMenuOption> subOptions = BuildSpikeOptions(surgeon, target, availableSpikes);
        if (subOptions.Count == 0) return null;

        return FloatSubMenuFactory.CompatMMMCreate(
            "CS_Hemalurgy_ChargeLiveSpike".Translate(target.LabelShortCap),
            subOptions
        );
    }

    private List<FloatMenuOption> BuildSpikeOptions(Pawn surgeon, Pawn target, List<Verse.Thing> spikes) {
        List<FloatMenuOption> options = [];
        for (int i = 0; i < spikes.Count; i++) {
            Verse.Thing spike = spikes[i];
            HemalurgicSpike? comp = spike.TryGetComp<HemalurgicSpike>();
            if (comp == null) continue;

            HemalurgicStealType stealType = comp.stealType;
            string spikeLabel = HemalurgicSpikeLabels.GetSpikeLabel(spike, comp, stealType);

            if (HemalurgicConstants.RequiresSelection(stealType)) {
                List<GeneDef> candidates = StealTargetSelector.GetStealCandidates(target, stealType);
                if (candidates.Count == 0) {
                    options.Add(
                        new FloatMenuOption(
                            spikeLabel + ": " + "CS_Hemalurgy_NothingToSteal".Translate(target.Named("DONOR")),
                            null
                        )
                    );
                    continue;
                }

                if (candidates.Count == 1) {
                    options.Add(
                        new FloatMenuOption(
                            spikeLabel + " (" + candidates[0].LabelCap + ")",
                            () => StartChargeJob(surgeon, target, spike, comp, candidates[0])
                        )
                    );
                    continue;
                }

                List<FloatMenuOption> geneOptions = [];
                for (int j = 0; j < candidates.Count; j++) {
                    GeneDef gene = candidates[j];
                    geneOptions.Add(
                        new FloatMenuOption(
                            gene.LabelCap,
                            () => StartChargeJob(surgeon, target, spike, comp, gene)
                        )
                    );
                }

                options.Add(FloatSubMenuFactory.CompatMMMCreate(spikeLabel, geneOptions));
            }
            else {
                options.Add(
                    new FloatMenuOption(
                        spikeLabel,
                        () => StartChargeJob(surgeon, target, spike, comp, null)
                    )
                );
            }
        }

        return options;
    }

    private void StartChargeJob(
        Pawn surgeon,
        Pawn target,
        Verse.Thing spike,
        HemalurgicSpike comp,
        GeneDef? selectedGene
    ) {
        comp.pendingStealTarget = selectedGene;
        HemalurgySurgeryJob.TryStart(surgeon, target, spike, JobDefOf.Cosmere_Scadrial_Job_ChargeLiveSpike);
    }

}