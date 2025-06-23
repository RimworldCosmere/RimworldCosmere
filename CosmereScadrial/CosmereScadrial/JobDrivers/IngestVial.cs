using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.JobDrivers;

public class IngestVial : JobDriver_Ingest {
    private Job? interruptedJob;

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref interruptedJob, "cosmere_interruptedJob");
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        // Save current job if this wasn't already queued
        if (interruptedJob == null && pawn.jobs?.curJob != job) {
            interruptedJob = pawn.jobs?.curJob;
        }

        foreach (Toil toil in base.MakeNewToils()) yield return toil;

        // After ingestion completes, requeue the saved job
        AddFinishAction(_ => {
            if (interruptedJob == null) return;
            pawn.jobs?.jobQueue?.EnqueueFirst(interruptedJob);
            interruptedJob = null;
        });
    }
}