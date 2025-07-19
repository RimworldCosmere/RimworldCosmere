using System;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Need;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

/// WIND RUNNER FLIGHT
public class SurgeGravitationProperties : CompProperties_AbilityEffect {
    public float stormLightCost;
    public ThingDef? thingDef;

    public SurgeGravitationProperties() {
        compClass = typeof(SurgeGravitation);
    }
}

public class SurgeGravitation : CompAbilityEffect {
    public new SurgeGravitationProperties props => (SurgeGravitationProperties)base.props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        // 1) Validate target
        if (target == null) {
            Log.Warning("[flight] Invalid target.");
            return;
        }

        // 2) Validate the "flyer" ThingDef
        if (props.thingDef == null) {
            Log.Error("[flight] No valid flyer ThingDef to spawn!");
            return;
        }

        Pawn caster = parent.pawn;
        if (caster == null) {
            return;
        }

        double distance = Math.Sqrt(
            Math.Pow(target.Cell.x - caster.Position.x, 2) + Math.Pow(target.Cell.z - caster.Position.z, 2)
        );
        float totalCost = (float)(props.stormLightCost * distance);

        if (!caster.TryGetComp(out Stormlight stormlight)) return;
        if (stormlight.currentStormlight < totalCost) {
            return;
        }

        stormlight.DrawStormlight(totalCost);

        // 3) Fling the target
        FlightFunction(caster.Map, target.Cell, distance);
    }


    private void FlightFunction(Map map, IntVec3 cell, double distance) {
        Pawn targetPawn = parent.pawn;
        if (targetPawn == null) {
            return;
        }

        //Log.Message($"TargetPawn: {targetPawn.Name}");
        PawnFlyer flyer = PawnFlyer.MakeFlyer(
            props.thingDef, // must have the <pawnFlyer> XML extension
            targetPawn, // the Pawn to fly
            cell, // an IntVec3 on the same map
            null, // optional visual effect
            null, // optional landing sound
            false, // whether the pawn’s carried item should come along
            null, // or a custom Vector3 start pos
            parent,
            LocalTargetInfo.Invalid
        );
        GenSpawn.Spawn(flyer, cell, map);
        RadiantUtility.GiveRadiantXp(targetPawn, (float)distance / 10f);
    }
}