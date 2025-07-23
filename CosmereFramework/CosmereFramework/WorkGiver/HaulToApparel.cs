using Cosmere.Framework.Thing;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.Framework.WorkGiver;

public class HaulToApparel : WorkGiver_HaulGeneral {
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

        if (haulDestination is not ApparelWithStorage apparelWithStorage) return null;
        if (!p.Spawned && !p.Equals(apparelWithStorage.SpawnedParentOrMe)) return null;

        ThingOwner interactableThingOwner = apparelWithStorage.TryGetInnerInteractableThingOwner();
        Job job = JobMaker.MakeJob(JobDefOf.Cosmere_HaulToApparelWithStorage, t, apparelWithStorage);
        job.count = Mathf.Min(t.stackCount, interactableThingOwner.GetCountCanAccept(t));
        job.haulMode = HaulMode.ToContainer;

        return job;
    }
}