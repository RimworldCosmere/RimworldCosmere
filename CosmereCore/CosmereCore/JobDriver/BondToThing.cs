using Cosmere.Core.Comp.Game;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Core.JobDriver;

public class BondToThing : Verse.AI.JobDriver {
    private const int TickInterval = GenTicks.TicksPerRealSecond;

    // ~1 every 11.1 in-game years (60000 ticks/day * 60 days/year)
    private const float ConnectionPerInterval = 0.000025f;
    private Verse.Thing Target => job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOn(() => Target is not { Spawned: true });

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        yield return new Toil {
            initAction = () => { pawn.AdjustConnection(Target, ConnectionPerInterval); },
            defaultCompleteMode = ToilCompleteMode.Instant,
            socialMode = RandomSocialMode.Normal,
            activeSkill = () => SkillDefOf.Social,
            handlingFacing = true,
        }.WithProgressBar(TargetIndex.A, () => pawn.GetConnection(Target).value);
    }
}