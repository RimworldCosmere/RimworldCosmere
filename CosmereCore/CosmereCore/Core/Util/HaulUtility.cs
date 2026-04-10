using System;
using Cosmere.Core.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Core.Util;

public static class HaulUtility {
    public static Job? PawnHaulThingToInnerStorage(Pawn pawn, Verse.Thing thing, IHaulDestination destination) {
        if (destination is not InnerStorage innerStorage) return null;
        if (!innerStorage.parent.SpawnedOrAnyParentSpawned) {
            return null;
        }

        if (innerStorage.ParentPawn != null && innerStorage.ParentPawn != pawn) return null;

        Job job = JobMaker.MakeJob(JobDefOf.Cosmere_HaulToInnerStorage, thing, innerStorage.parent);
        job.count = Math.Min(thing.stackCount, innerStorage.GetCountCanAccept(thing));
        job.haulMode = HaulMode.ToContainer;

        return job;
    }
}