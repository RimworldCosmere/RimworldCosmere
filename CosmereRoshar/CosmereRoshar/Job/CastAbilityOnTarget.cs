using RimWorld;
using Verse;

namespace CosmereRoshar.Job;

public class CastAbilityOnTarget : Verse.AI.Job {
    public Ability abilityToCast;

    public CastAbilityOnTarget() { }

    public CastAbilityOnTarget(JobDef def, LocalTargetInfo targetA, Ability ability) : base(def, targetA) {
        abilityToCast = ability;
    }
}