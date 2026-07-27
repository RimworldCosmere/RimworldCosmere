using Cosmere.Core.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.Core.WorkGiver;

public class StoreInApparelInnerStorage : WorkGiver_HaulGeneral {
    public override Job? JobOnThing(Pawn p, Verse.Thing t, bool forced = false) {
        StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(t, forced);
        if (!StoreUtility.TryFindBestBetterStorageFor(
                t,
                p,
                p.Map,
                currentPriority,
                p.Faction,
                out _,
                out IHaulDestination haulDestination
            )) {
            JobFailReason.Is(HaulAIUtility.NoEmptyPlaceLowerTrans);
            return null;
        }

        if (haulDestination is not InnerStorage innerStorage) return null;
        if (innerStorage.parent is not Apparel apparel) return null;
        if (!apparel.Spawned && !p.Equals(innerStorage.parent)) return null;

        Job job = JobMaker.MakeJob(JobDefOf.Cosmere_StoreInApparelInnerStorage, t, innerStorage.parent);
        job.count = Mathf.Min(t.stackCount, innerStorage.innerContainer!.GetCountCanAccept(t));
        job.haulMode = HaulMode.ToContainer;

        return job;
    }
}
