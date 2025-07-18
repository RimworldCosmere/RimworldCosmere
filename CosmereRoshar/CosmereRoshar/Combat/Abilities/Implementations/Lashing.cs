using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar;

[HarmonyPatch(typeof(PawnFlyer))]
[HarmonyPatch("RespawnPawn")]
public class Patch_PP {
    private static Pawn flyingPawn;

    private static void Prefix(ThingOwner<Thing> ___innerContainer) {
        if (___innerContainer.InnerListForReading.Count <= 0) {
            return;
        }

        flyingPawn = ___innerContainer.InnerListForReading[0] as Pawn;
    }

    private static void Postfix(AbilityDef ___triggeringAbility) {
        if (___triggeringAbility != null && flyingPawn != null) {
            if (___triggeringAbility.defName == CosmereRosharDefs.whtwl_LashingUpward.defName) {
                flyingPawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 75));
            }
        }
    }
}

/// LASH UP ABILITY
public class CompProperties_AbilityLashUpward : CompProperties_AbilityEffect {
    public float stormLightCost;
    public ThingDef thingDef;

    public CompProperties_AbilityLashUpward() {
        compClass = typeof(CompAbilityEffect_AbilityLashUpward);
    }
}

public class CompAbilityEffect_AbilityLashUpward : CompAbilityEffect {
    public new CompProperties_AbilityLashUpward Props => props as CompProperties_AbilityLashUpward;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        // 1) Validate target
        if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
            Log.Warning("[LashUpward] Invalid target.");
            return;
        }

        // 2) Validate the "flyer" ThingDef
        if (Props.thingDef == null) {
            Log.Error("[LashUpward] No valid flyer ThingDef to spawn!");
            return;
        }

        Pawn caster = parent.pawn;
        if (caster == null || caster == target) {
            return;
        }

        if (caster.GetComp<CompStormlight>() == null ||
            caster.GetComp<CompStormlight>().Stormlight < Props.stormLightCost) {
            //Log.Message($"[LashUpward] not enough stormlight!");
            return;
        }

        caster.GetComp<CompStormlight>().drawStormlight(Props.stormLightCost);

        // 3) Fling the target
        flightFunction(target.Thing);
    }


    private void flightFunction(Thing targetThing) {
        Map map = targetThing.Map;
        IntVec3 cell = targetThing.Position;

        Pawn targetPawn = targetThing as Pawn;
        if (targetPawn == null) {
            return;
        }

        //Log.Message($"TargetPawn: {targetPawn.Name}");
        // Create the custom flyer
        PawnFlyer flyer = PawnFlyer.MakeFlyer(
            Props.thingDef, // must have the <pawnFlyer> XML extension
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
            RadiantUtility.GiveRadiantXP(caster, 50f);
        }
    }
}