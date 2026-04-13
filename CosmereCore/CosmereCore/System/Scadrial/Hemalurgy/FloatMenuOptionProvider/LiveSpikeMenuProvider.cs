using System.Collections.Generic;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using Cosmere.System.Scadrial.Hemalurgy.Dialog;
using FloatSubMenus;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Hemalurgy.FloatMenuOptionProvider;

public class LiveSpikeMenuProvider : RimWorld.FloatMenuOptionProvider {
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

        List<Verse.Thing> availableSpikes = FindAllUnchargedSpikes(surgeon);
        if (availableSpikes.Count == 0) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ChargeLiveSpike".Translate(target.LabelShortCap) + ": " + "CS_Hemalurgy_NoUnchargedSpike".Translate(),
                null
            );
        }

        if (!surgeon.CanReach(target, PathEndMode.ClosestTouch, Danger.Deadly)) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ChargeLiveSpike".Translate(target.LabelShortCap) + ": " + "NoPath".Translate().CapitalizeFirst(),
                null
            );
        }

        List<FloatMenuOption> subOptions = BuildSpikeOptions(surgeon, target, availableSpikes);
        if (subOptions.Count == 0) return null;

        return FloatSubMenu.CompatMMMCreate(
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

            bool isNeedle = spike.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;
            string label = isNeedle ? "CS_Hemalurgy_NeedleOption" : "CS_Hemalurgy_SpikeOption";
            string metalLabel = comp.metal?.LabelCap ?? "unknown";
            HemalurgicStealType stealType = comp.stealType;
            string stealLabel = HemalurgicConstants.GetStealTypeLabel(stealType);
            string spikeLabel = label.Translate(metalLabel, stealLabel);

            if (HemalurgicConstants.RequiresSelection(stealType)) {
                List<GeneDef> candidates = StealTargetSelector.GetStealCandidates(target, stealType);
                if (candidates.Count == 0) {
                    options.Add(new FloatMenuOption(
                        spikeLabel + ": " + "CS_Hemalurgy_NothingToSteal".Translate(target.Named("DONOR")),
                        null
                    ));
                    continue;
                }

                if (candidates.Count == 1) {
                    options.Add(new FloatMenuOption(
                        spikeLabel + " (" + candidates[0].LabelCap + ")",
                        () => StartChargeJob(surgeon, target, spike, comp, candidates[0])
                    ));
                    continue;
                }

                List<FloatMenuOption> geneOptions = [];
                for (int j = 0; j < candidates.Count; j++) {
                    GeneDef gene = candidates[j];
                    geneOptions.Add(new FloatMenuOption(
                        gene.LabelCap,
                        () => StartChargeJob(surgeon, target, spike, comp, gene)
                    ));
                }
                options.Add(FloatSubMenu.CompatMMMCreate(spikeLabel, geneOptions));
            } else {
                options.Add(new FloatMenuOption(
                    spikeLabel,
                    () => StartChargeJob(surgeon, target, spike, comp, null)
                ));
            }
        }

        return options;
    }

    private void StartChargeJob(Pawn surgeon, Pawn target, Verse.Thing spike, HemalurgicSpike comp, GeneDef? selectedGene) {
        if (!surgeon.CanReach(spike, PathEndMode.ClosestTouch, Danger.Deadly)) {
            Messages.Message("NoPath".Translate().CapitalizeFirst(), MessageTypeDefOf.RejectInput);
            return;
        }

        Building_Bed? bed = FindBedForDonor(target, surgeon);
        if (bed == null && !target.InBed()) {
            Messages.Message(
                "CS_Hemalurgy_NoBedForDonor".Translate(target.Named("DONOR")),
                target, MessageTypeDefOf.RejectInput
            );
            return;
        }

        LocalTargetInfo bedTarget = bed != null ? (LocalTargetInfo)bed : LocalTargetInfo.Invalid;

        comp.pendingStealTarget = selectedGene;

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
            Scadrial.JobDefOf.Cosmere_Scadrial_Job_ChargeLiveSpike, target, spike, bedTarget
        );
        job.count = 1;
        surgeon.jobs.TryTakeOrderedJob(job);
    }

    internal static Building_Bed? FindBedForDonor(Pawn donor, Pawn surgeon) {
        if (donor.InBed()) return donor.CurrentBed();

        Map map = donor.Map;
        Building_Bed? bestMedBed = null;
        float bestDist = float.MaxValue;

        List<Building> buildings = map.listerBuildings.allBuildingsColonist;
        for (int i = 0; i < buildings.Count; i++) {
            if (buildings[i] is not Building_Bed bed) continue;
            if (!bed.Medical) continue;
            if (!bed.AnyUnoccupiedSleepingSlot) continue;
            if (!donor.CanReserve(bed)) continue;
            if (!surgeon.CanReach(bed, PathEndMode.InteractionCell, Danger.Deadly)) continue;

            float dist = bed.Position.DistanceToSquared(donor.Position);
            if (dist < bestDist) {
                bestDist = dist;
                bestMedBed = bed;
            }
        }

        if (bestMedBed != null) return bestMedBed;

        GuestStatus? guestStatus = null;
        if (donor.IsPrisonerOfColony) guestStatus = GuestStatus.Prisoner;
        else if (donor.IsSlaveOfColony) guestStatus = GuestStatus.Slave;

        return RestUtility.FindBedFor(donor, surgeon, false, false, guestStatus);
    }

    private List<Verse.Thing> FindAllUnchargedSpikes(Pawn pawn) {
        Map map = pawn.Map;
        List<Verse.Thing> result = [];

        List<Verse.Thing> spikes = map.listerThings.ThingsOfDef(HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike);
        for (int i = 0; i < spikes.Count; i++) {
            HemalurgicSpike? comp = spikes[i].TryGetComp<HemalurgicSpike>();
            if (comp == null || comp.isCharged) continue;
            if (spikes[i].IsForbidden(pawn)) continue;
            result.Add(spikes[i]);
        }

        List<Verse.Thing> needles = map.listerThings.ThingsOfDef(HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle);
        for (int i = 0; i < needles.Count; i++) {
            HemalurgicSpike? comp = needles[i].TryGetComp<HemalurgicSpike>();
            if (comp == null || comp.isCharged) continue;
            if (needles[i].IsForbidden(pawn)) continue;
            result.Add(needles[i]);
        }

        return result;
    }
}
