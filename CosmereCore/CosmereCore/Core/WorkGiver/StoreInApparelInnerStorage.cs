using Cosmere.Core.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.Core.WorkGiver;

/// <summary>
///     Fills the storage a pawn is wearing. Worn storage is off the map-wide haul search on
///     purpose, so this looks at the wearer's own apparel instead of asking StoreUtility.
/// </summary>
public class StoreInApparelInnerStorage : WorkGiver_HaulGeneral {
    public override Job? JobOnThing(Pawn p, Verse.Thing t, bool forced = false) {
        if (t is Corpse || p.apparel == null) return null;
        if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(p, t, forced)) return null;

        StoragePriority current = StoreUtility.CurrentStoragePriorityOf(t, forced);
        InnerStorage? best = null;
        foreach (Apparel worn in p.apparel.WornApparel) {
            InnerStorage? pouch = worn.TryGetComp<InnerStorage>();
            if (pouch == null || !pouch.Accepts(t)) continue;

            StoragePriority priority = pouch.GetStoreSettings().Priority;
            if (priority <= current) continue;
            if (best == null || priority > best.GetStoreSettings().Priority) best = pouch;
        }

        if (best == null) return null;

        Job job = JobMaker.MakeJob(JobDefOf.Cosmere_StoreInApparelInnerStorage, t, best.parent);
        job.count = Mathf.Min(t.stackCount, best.GetCountCanAccept(t));
        job.haulMode = HaulMode.ToContainer;

        return job;
    }
}
