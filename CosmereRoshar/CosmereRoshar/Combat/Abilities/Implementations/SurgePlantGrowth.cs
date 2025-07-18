using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Need;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

/// Surge regrowth plants
public class CompPropertiesAbilitySurgePlantGrowth : CompProperties_AbilityEffect {
    public float stormLightCost;

    public CompPropertiesAbilitySurgePlantGrowth() {
        compClass = typeof(CompAbilityEffectSurgePlantGrowth);
    }
}

public class CompAbilityEffectSurgePlantGrowth : CompAbilityEffect {
    public new CompPropertiesAbilitySurgePlantGrowth props => ((AbilityComp)this).props as CompPropertiesAbilitySurgePlantGrowth;

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


    private void GrowthFunction(Thing targetThing) {
        Plant? targetPlant = targetThing as Plant;
        if (targetPlant == null) {
            return;
        }

        Pawn caster = parent.pawn;
        Stormlight? stormlight = caster?.TryGetComp<Stormlight>();
        if (stormlight == null || !(stormlight.currentStormlight >= props.stormLightCost)) return;
        targetPlant.Growth = 1;
        RadiantUtility.GiveRadiantXp(caster, 50f);
        stormlight.DrawStormlight(props.stormLightCost);
    }
}