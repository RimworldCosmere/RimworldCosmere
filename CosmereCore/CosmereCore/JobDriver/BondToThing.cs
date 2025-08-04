using Cosmere.Core.Comp.Game;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.Core.JobDriver;

public class BondToThing : Verse.AI.JobDriver {
    private const int TickInterval = GenTicks.TicksPerRealSecond;
    private const int MaxTicks = GenTicks.TicksPerRealSecond * 5;

    // ~1 every 11.1 in-game years (60000 ticks/day * 60 days/year)
    private const float ConnectionPerInterval = 0.0025f;
    private int startTick;
    private Verse.Thing Target => job.targetB.Thing;
    private Connection connection => pawn.GetConnection(Target);

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOn(() => Target is not { Spawned: true });

        Connection conn = pawn.GetConnection(Target);
        this.FailOn(() => conn.value >= 1);

        //yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

        yield return new Toil {
            initAction = () => {
                if (pawn.DistanceTo(Target) <= 5) return;

                IntVec3 cell = CellFinder.RandomSpawnCellForPawnNear(Target.Position, Target.Map, 5);
                pawn.pather.StartPath(cell, PathEndMode.Touch);
            },
        };

        Toil bondToil = new Toil {
            initAction = () => startTick = GenTicks.TicksGame,
            tickIntervalAction = delta => {
                if (GenTicks.TicksGame > startTick + MaxTicks || pawn.DistanceTo(Target) > 5) {
                    EndJobWith(JobCondition.Succeeded);
                    return;
                }

                if (!pawn.IsHashIntervalTick(TickInterval, delta)) return;

                pawn.AdjustConnection(Target, ConnectionPerInterval);
                pawn.rotationTracker.FaceTarget(Target);

                if (conn.value >= 1f) {
                    EndJobWith(JobCondition.Succeeded);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Never,
            socialMode = RandomSocialMode.Quiet,
            activeSkill = () => SkillDefOf.Social,
            handlingFacing = true,
        };

        bondToil.WithProgressBar(
            TargetIndex.A,
            () => {
                float elapsed = GenTicks.TicksGame - startTick;
                return Mathf.Clamp01(elapsed / MaxTicks);
            }
        );

        bondToil.WithProgressBar(TargetIndex.B, () => Mathf.Clamp01(connection.value / 1f));

        yield return bondToil;
    }
}