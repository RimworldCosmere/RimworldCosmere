using Verse.AI;

namespace Cosmere.Framework.Extension;

public static class Pawn_JobTrackerExtension {
    public static Job InterruptJobWith(this Pawn_JobTracker jobs, Job job, JobTag? tag = null) {
        jobs.StartJob(job, JobCondition.Ongoing, null, true);

        return job;
    }
}