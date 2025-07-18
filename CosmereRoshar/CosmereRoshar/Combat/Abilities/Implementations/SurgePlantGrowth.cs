using RimWorld;
using Verse;

namespace CosmereRoshar;

/// Surge regrowth plants
public class CompProperties_AbilitySurgePlantGrowth : CompProperties_AbilityEffect {
    public float stormLightCost;

    public CompProperties_AbilitySurgePlantGrowth() {
        compClass = typeof(CompAbilityEffect_SurgePlantGrowth);
    }
}

public class CompAbilityEffect_SurgePlantGrowth : CompAbilityEffect {
    public new CompProperties_AbilitySurgePlantGrowth Props => props as CompProperties_AbilitySurgePlantGrowth;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
            Log.Warning("[plant growth] Invalid target.");
            return;
        }

        Pawn caster = parent.pawn;
        if (caster == null) {
            return;
        }

        if (caster.GetComp<CompStormlight>() == null) {
            Log.Message("[plant growth] null!");
            return;
        }

        growthFunction(target.Thing);
    }


    private void growthFunction(Thing targetThing) {
        Map map = targetThing.Map;
        IntVec3 cell = targetThing.Position;

        Plant targetPlant = targetThing as Plant;
        if (targetPlant == null) {
            return;
        }

        Pawn caster = parent.pawn;
        if (caster != null) {
            CompStormlight casterComp = caster.GetComp<CompStormlight>();
            if (casterComp != null && casterComp.Stormlight >= Props.stormLightCost) {
                targetPlant.Growth = 1;
                RadiantUtility.GiveRadiantXP(caster, 50f);
                casterComp.drawStormlight(Props.stormLightCost);
            }
        }
    }
}