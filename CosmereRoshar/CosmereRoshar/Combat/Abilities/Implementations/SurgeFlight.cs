using System;
using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar;

/// WIND RUNNER FLIGHT
public class CompProperties_AbilityWindRunnerFlight : CompProperties_AbilityEffect {
    public float stormLightCost;
    public ThingDef thingDef;

    public CompProperties_AbilityWindRunnerFlight() {
        compClass = typeof(CompAbilityEffect_AbilityWindRunnerFlight);
    }
}

public class CompAbilityEffect_AbilityWindRunnerFlight : CompAbilityEffect {
    public new CompProperties_AbilityWindRunnerFlight Props => props as CompProperties_AbilityWindRunnerFlight;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        // 1) Validate target
        if (target == null || target.Cell == null) {
            Log.Warning("[flight] Invalid target.");
            return;
        }

        // 2) Validate the "flyer" ThingDef
        if (Props.thingDef == null) {
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
        float totalCost = (float)(Props.stormLightCost * distance);

        if (caster.GetComp<CompStormlight>() == null || caster.GetComp<CompStormlight>().Stormlight < totalCost) {
            return;
        }

        caster.GetComp<CompStormlight>().drawStormlight(totalCost);

        // 3) Fling the target
        flightFunction(caster.Map, target.Cell, distance);
    }


    private void flightFunction(Map map, IntVec3 cell, double distance) {
        Pawn targetPawn = parent.pawn;
        if (targetPawn == null) {
            return;
        }

        //Log.Message($"TargetPawn: {targetPawn.Name}");
        PawnFlyer flyer = PawnFlyer.MakeFlyer(
            Props.thingDef, // must have the <pawnFlyer> XML extension
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
        RadiantUtility.GiveRadiantXP(targetPawn, (float)distance / 10f);
    }
}