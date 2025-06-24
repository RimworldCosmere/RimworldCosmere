using Verse.AI;

namespace CosmereFramework.Extensions;

public static class Pawn_JobTrackerExtension {
    public static Job InterruptJobWith(this Pawn_JobTracker jobs, Job job, JobTag? tag = null) {
        Job jobToInterrupt = jobs.curJob;
        if (jobToInterrupt != null) {
            //jobs.EndCurrentJob(JobCondition.InterruptForced, false);
        }

        jobs.StartJob(job, JobCondition.Ongoing, null, true);
        if (jobToInterrupt != null) {
            //jobs.curDriver.AddFinishAction(_ => jobs.TryTakeOrderedJob(jobToInterrupt));
        }

        return job;
    }
}