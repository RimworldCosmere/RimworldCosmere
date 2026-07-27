using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Hemalurgy.Util;

public static class HemalurgySurgeryJob {
    public static List<Verse.Thing> FindSpikes(Pawn pawn, bool charged) {
        Map map = pawn.Map;
        List<Verse.Thing> result = [];
        AppendByDef(map, pawn, charged, HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike, result);
        AppendByDef(map, pawn, charged, HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle, result);
        return result;
    }

    public static Building_Bed? FindBedForDonor(Pawn donor, Pawn surgeon) {
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
        if (donor.IsPrisonerOfColony) {
            guestStatus = GuestStatus.Prisoner;
        } else if (donor.IsSlaveOfColony) {
            guestStatus = GuestStatus.Slave;
        }

        return RestUtility.FindBedFor(donor, surgeon, false, false, guestStatus);
    }

    public static bool TryStart(Pawn surgeon, Pawn target, Verse.Thing spike, JobDef jobDef) {
        if (!surgeon.CanReach(spike, PathEndMode.ClosestTouch, Danger.Deadly)) {
            Messages.Message("NoPath".Translate().CapitalizeFirst(), MessageTypeDefOf.RejectInput);
            return false;
        }

        Building_Bed? bed = FindBedForDonor(target, surgeon);
        if (bed == null && !target.InBed()) {
            Messages.Message(
                "CS_Hemalurgy_NoBedForDonor".Translate(target.Named("DONOR")),
                target,
                MessageTypeDefOf.RejectInput
            );
            return false;
        }

        LocalTargetInfo bedTarget = bed != null ? (LocalTargetInfo)bed : LocalTargetInfo.Invalid;

        bool isVoluntary = target.Faction == Faction.OfPlayer
                           && !target.IsPrisonerOfColony
                           && !target.IsSlaveOfColony;
        if (isVoluntary && !target.InBed() && !target.Downed && bed != null) {
            Verse.AI.Job donorJob = JobMaker.MakeJob(JobDefOf.Cosmere_Scadrial_Job_WaitInBed, bed);
            target.jobs.TryTakeOrderedJob(donorJob);
        }

        Verse.AI.Job job = JobMaker.MakeJob(jobDef, target, spike, bedTarget);
        job.count = 1;
        surgeon.jobs.TryTakeOrderedJob(job);
        return true;
    }

    private static void AppendByDef(Map map, Pawn pawn, bool charged, ThingDef def, List<Verse.Thing> result) {
        List<Verse.Thing> things = map.listerThings.ThingsOfDef(def);
        for (int i = 0; i < things.Count; i++) {
            HemalurgicSpike? comp = things[i].TryGetComp<HemalurgicSpike>();
            if (comp == null || comp.isCharged != charged) continue;
            if (things[i].IsForbidden(pawn)) continue;
            result.Add(things[i]);
        }
    }
}
