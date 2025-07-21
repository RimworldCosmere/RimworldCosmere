using Verse;

namespace Cosmere.Roshar.Job;

public class CastAbilityOnTarget : Verse.AI.Job {
    public RimWorld.Ability abilityToCast;

    public CastAbilityOnTarget() { }

    public CastAbilityOnTarget(JobDef def, LocalTargetInfo targetA, RimWorld.Ability ability) : base(def, targetA) {
        abilityToCast = ability;
    }
}