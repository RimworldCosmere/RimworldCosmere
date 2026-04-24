using Cosmere.System.Roshar.Surgebinding.Ability;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Job.Driver;

public class CastInstant : JobDriver {
    private SurgebindingAbility ability => (SurgebindingAbility)job.source!;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return true;
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        Verse.AI.Toil castToil = ToilMaker.MakeToil(nameof(CastInstant));
        castToil.initAction = () => { ability.Activate(TargetA, TargetB); };
        castToil.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return castToil;
    }
}