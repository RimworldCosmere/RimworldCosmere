using Cosmere.Roshar.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Combat.Abilities.Implementations;

/// Surge regrowth plants
public class SurgePlantGrowthProperties : CompProperties_AbilityEffect {
    public float stormLightCost;

    public SurgePlantGrowthProperties() {
        compClass = typeof(SurgePlantGrowth);
    }
}

public class SurgePlantGrowth : CompAbilityEffect {
    public new SurgePlantGrowthProperties props => (SurgePlantGrowthProperties)base.props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
            Log.Warning("[plant growth] Invalid target.");
            return;
        }

        Pawn caster = parent.pawn;
        if (caster == null) {
            return;
        }

        if (caster.GetComp<Stormlight>() == null) {
            Log.Message("[plant growth] null!");
            return;
        }

        GrowthFunction(target.Thing);
    }


    private void GrowthFunction(Verse.Thing targetThing) {
        Plant? targetPlant = targetThing as Plant;
        if (targetPlant == null) {
            return;
        }

        Pawn caster = parent.pawn;
        Stormlight? stormlight = caster?.TryGetComp<Stormlight>();
        if (stormlight == null || !(stormlight.currentStormlight >= props.stormLightCost)) return;
        targetPlant.Growth = 1;
        caster!.skills.Learn(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower, .25f);
        stormlight.DrawStormlight(props.stormLightCost);
    }
}