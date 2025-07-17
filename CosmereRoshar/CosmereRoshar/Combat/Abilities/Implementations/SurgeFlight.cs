using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;


namespace CosmereRoshar {

    /// WIND RUNNER FLIGHT
    public class CompProperties_AbilityWindRunnerFlight : CompProperties_AbilityEffect {
        public ThingDef thingDef;
        public float stormLightCost;

        public CompProperties_AbilityWindRunnerFlight() {
            this.compClass = typeof(CompAbilityEffect_AbilityWindRunnerFlight);
        }
    }
    public class CompAbilityEffect_AbilityWindRunnerFlight : CompAbilityEffect {
        public new CompProperties_AbilityWindRunnerFlight Props => this.props as CompProperties_AbilityWindRunnerFlight;

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

            Pawn caster = this.parent.pawn as Pawn;
            if (caster == null) {
                return;
            }

            double distance = Math.Sqrt(Math.Pow(target.Cell.x - caster.Position.x, 2) + Math.Pow(target.Cell.z - caster.Position.z, 2));
            float totalCost = (float)(Props.stormLightCost * distance);

            if (caster.GetComp<CompStormlight>() == null || caster.GetComp<CompStormlight>().Stormlight < totalCost) {
                return;
            }
            caster.GetComp<CompStormlight>().drawStormlight(totalCost);

            // 3) Fling the target
            flightFunction(caster.Map, target.Cell, distance);
        }



        private void flightFunction(Map map, IntVec3 cell, double distance) {

            Pawn targetPawn = this.parent.pawn as Pawn;
            if (targetPawn == null) {
                return;
            }

            //Log.Message($"TargetPawn: {targetPawn.Name}");
            PawnFlyer flyer = PawnFlyer.MakeFlyer(
              flyingDef: Props.thingDef,    // must have the <pawnFlyer> XML extension
              pawn: targetPawn,             // the Pawn to fly
              destCell: cell,               // an IntVec3 on the same map
              flightEffecterDef: null,      // optional visual effect
              landingSound: null,           // optional landing sound
              flyWithCarriedThing: false,   // whether the pawn’s carried item should come along
              overrideStartVec: null,       // or a custom Vector3 start pos
              triggeringAbility: this.parent,
              target: LocalTargetInfo.Invalid
          );
            GenSpawn.Spawn(flyer, cell, map);
            RadiantUtility.GiveRadiantXP(targetPawn, (float)distance / 10f);
        }
    }
}