using System.Collections.Generic;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using FloatSubMenus;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Hemalurgy.FloatMenuOptionProvider;

public class ImplantSpikeMenuProvider : RimWorld.FloatMenuOptionProvider {
    protected override bool Drafted => false;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    protected override FloatMenuOption? GetSingleOptionFor(Verse.Thing clickedThing, FloatMenuContext context) {
        if (clickedThing is not Pawn target) return null;
        if (!target.RaceProps.Humanlike) return null;
        if (target.Dead) return null;

        Pawn? surgeon = context.FirstSelectedPawn;
        if (surgeon == null) return null;
        if (surgeon == target) return null;

        if (!ResearchProjectDef.Named("Cosmere_Scadrial_Hemalurgy").IsFinished) return null;

        List<Verse.Thing> chargedSpikes = FindAllChargedSpikes(surgeon);
        if (chargedSpikes.Count == 0) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ImplantLiveSpike".Translate(target.LabelShortCap) + ": " + "CS_Hemalurgy_NoChargedSpike".Translate(),
                null
            );
        }

        if (!surgeon.CanReach(target, PathEndMode.ClosestTouch, Danger.Deadly)) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ImplantLiveSpike".Translate(target.LabelShortCap) + ": " + "NoPath".Translate().CapitalizeFirst(),
                null
            );
        }

        List<FloatMenuOption> subOptions = BuildSpikeOptions(surgeon, target, chargedSpikes);
        if (subOptions.Count == 0) return null;

        return FloatSubMenu.CompatMMMCreate(
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

            if (comp.chargeData!.stealType == HemalurgicStealType.RemoveAllPowers) {
                options.Add(new FloatMenuOption(
                    GetSpikeLabel(spike, comp) + ": " + "CS_Hemalurgy_CannotImplantAluminum".Translate(),
                    null
                ));
                continue;
            }

            string label = GetSpikeLabel(spike, comp);

            options.Add(new FloatMenuOption(
                label,
                () => {
                    if (!surgeon.CanReach(spike, PathEndMode.ClosestTouch, Danger.Deadly)) {
                        Messages.Message("NoPath".Translate().CapitalizeFirst(), MessageTypeDefOf.RejectInput);
                        return;
                    }

                    Building_Bed? bed = LiveSpikeMenuProvider.FindBedForDonor(target, surgeon);
                    if (bed == null && !target.InBed()) {
                        Messages.Message(
                            "CS_Hemalurgy_NoBedForDonor".Translate(target.Named("DONOR")),
                            target, MessageTypeDefOf.RejectInput
                        );
                        return;
                    }

                    LocalTargetInfo bedTarget = bed != null ? (LocalTargetInfo)bed : LocalTargetInfo.Invalid;

                    bool isVoluntary = target.Faction == Faction.OfPlayer
                        && !target.IsPrisonerOfColony
                        && !target.IsSlaveOfColony;
                    if (isVoluntary && !target.InBed() && !target.Downed && bed != null) {
                        Verse.AI.Job donorJob = JobMaker.MakeJob(
                            Scadrial.JobDefOf.Cosmere_Scadrial_Job_WaitInBed, bed
                        );
                        target.jobs.TryTakeOrderedJob(donorJob);
                    }

                    Verse.AI.Job job = JobMaker.MakeJob(
                        Scadrial.JobDefOf.Cosmere_Scadrial_Job_ImplantSpike, target, spike, bedTarget
                    );
                    job.count = 1;
                    surgeon.jobs.TryTakeOrderedJob(job);
                }
            ));
        }

        return options;
    }

    private string GetSpikeLabel(Verse.Thing spike, HemalurgicSpike comp) {
        bool isNeedle = spike.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;
        string label = isNeedle ? "CS_Hemalurgy_NeedleOption" : "CS_Hemalurgy_SpikeOption";
        string metalLabel = comp.metal.LabelCap;
        HemalurgicStealType stealType = comp.chargeData!.stealType;
        string stealLabel = HemalurgicConstants.GetStealTypeLabel(stealType);
        return label.Translate(metalLabel, stealLabel);
    }

    private List<Verse.Thing> FindAllChargedSpikes(Pawn pawn) {
        Map map = pawn.Map;
        List<Verse.Thing> result = [];

        List<Verse.Thing> spikes = map.listerThings.ThingsOfDef(HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike);
        for (int i = 0; i < spikes.Count; i++) {
            HemalurgicSpike? comp = spikes[i].TryGetComp<HemalurgicSpike>();
            if (comp == null || !comp.isCharged) continue;
            if (spikes[i].IsForbidden(pawn)) continue;
            result.Add(spikes[i]);
        }

        List<Verse.Thing> needles = map.listerThings.ThingsOfDef(HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle);
        for (int i = 0; i < needles.Count; i++) {
            HemalurgicSpike? comp = needles[i].TryGetComp<HemalurgicSpike>();
            if (comp == null || !comp.isCharged) continue;
            if (needles[i].IsForbidden(pawn)) continue;
            result.Add(needles[i]);
        }

        return result;
    }
}
