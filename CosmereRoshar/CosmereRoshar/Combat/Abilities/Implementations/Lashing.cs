using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Need;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

[HarmonyPatch(typeof(PawnFlyer))]
[HarmonyPatch("RespawnPawn")]
public class PatchPp {
    private static Pawn FlyingPawn;

    private static void Prefix(ThingOwner<Thing> innerContainer) {
        if (innerContainer.InnerListForReading.Count <= 0) {
            return;
        }

        FlyingPawn = innerContainer.InnerListForReading[0] as Pawn;
    }

    private static void Postfix(AbilityDef triggeringAbility) {
        if (triggeringAbility != null && FlyingPawn != null) {
            if (triggeringAbility.defName == CosmereRosharDefs.WhtwlLashingUpward.defName) {
                FlyingPawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 75));
            }
        }
    }
}

/// LASH UP ABILITY
public class CompPropertiesAbilityLashUpward : CompProperties_AbilityEffect {
    public float stormLightCost;
    public ThingDef thingDef;

    public CompPropertiesAbilityLashUpward() {
        compClass = typeof(CompAbilityEffectAbilityLashUpward);
    }
}

public class CompAbilityEffectAbilityLashUpward : CompAbilityEffect {
    public new CompPropertiesAbilityLashUpward props => ((AbilityComp)this).props as CompPropertiesAbilityLashUpward;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        // 1) Validate target
        if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
            Log.Warning("[LashUpward] Invalid target.");
            return;
        }

        // 2) Validate the "flyer" ThingDef
        if (props.thingDef == null) {
            Log.Error("[LashUpward] No valid flyer ThingDef to spawn!");
            return;
        }

        Pawn caster = parent.pawn;
        if (caster == null || caster == target) {
            return;
        }

        if (caster.TryGetComp<Stormlight>()?.currentStormlight < props.stormLightCost) {
            //Log.Message($"[LashUpward] not enough stormlight!");
            return;
        }

        caster.GetComp<Stormlight>().DrawStormlight(props.stormLightCost);

        // 3) Fling the target
        FlightFunction(target.Thing);
    }


    private void FlightFunction(Thing targetThing) {
        Map map = targetThing.Map;
        IntVec3 cell = targetThing.Position;

        Pawn targetPawn = targetThing as Pawn;
        if (targetPawn == null) {
            return;
        }

        //Log.Message($"TargetPawn: {targetPawn.Name}");
        // Create the custom flyer
        PawnFlyer flyer = PawnFlyer.MakeFlyer(
            props.thingDef, // must have the <pawnFlyer> XML extension
            targetPawn, // the Pawn to fly
            cell, // an IntVec3 on the same map
            null, // optional visual effect
            null, // optional landing sound
            false, // whether the pawn’s carried item should come along
            null, // or a custom Vector3 start pos
            parent, // pass an Ability if relevant
            LocalTargetInfo.Invalid
        );

        GenSpawn.Spawn(flyer, cell, map);
        Pawn caster = parent.pawn;
        if (caster != null) {
            RadiantUtility.GiveRadiantXp(caster, 50f);
        }
    }
}