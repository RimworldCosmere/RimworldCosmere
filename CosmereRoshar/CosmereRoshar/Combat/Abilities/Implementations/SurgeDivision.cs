using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereRoshar;

public class CompProperties_AbilitySurgeDivision : CompProperties_AbilityEffect {
    public float stormLightCost;

    public CompProperties_AbilitySurgeDivision() {
        compClass = typeof(CompAbilityEffect_SurgeDivision);
    }
}

public class CompAbilityEffect_SurgeDivision : CompAbilityEffect {
    public new CompProperties_AbilitySurgeDivision Props => props as CompProperties_AbilitySurgeDivision;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        // 1) Validate target
        if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
            Log.Warning("[Division surge] Invalid target.");
            return;
        }

        Pawn caster = parent.pawn;
        if (caster == null) {
            Log.Error("[Division surge] caster is null!");
            return;
        }

        if (caster.GetComp<CompStormlight>() == null) {
            Log.Error("[Division surge] no stormlight comp!");
            return;
        }

        tryToUseDivision(target.Thing as Pawn, caster);
    }

    private void tryToUseDivision(Pawn victim, Pawn caster) {
        if (victim == null || caster.GetComp<CompStormlight>().Stormlight < Props.stormLightCost) return;
        List<Thing> thingsIgnoredByExplosion = new List<Thing> { caster };
        if (StormlightUtilities.RollTheDice(1, 25, 2) &&
            caster.GetComp<CompStormlight>().Stormlight >= Props.stormLightCost * 1.5f) {
            //GenExplosion.DoExplosion(victim.Position, caster.Map, 1.0f, DamageDefOf.Vaporize, caster, -1, -1f, null, null, null, null, null, 0f, 1, null, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 0f, damageFalloff: false, null, thingsIgnoredByExplosion, null);
            GenExplosion.DoExplosion(
                victim.Position,
                caster.Map,
                1.0f,
                DamageDefOf.Vaporize,
                caster,
                -1,
                -1f,
                null,
                null,
                null,
                null,
                null,
                0f,
                1,
                null, // new -1-
                null, // new -2-
                255, // new -3-  (default)
                false,
                null,
                0f,
                1,
                0f,
                false,
                null,
                thingsIgnoredByExplosion
            );
            caster.GetComp<CompStormlight>().drawStormlight(Props.stormLightCost * 1.5f);
        } else {
            victim.TryAttachFire(Rand.Range(0.95f, 0.99f), caster);
            RadiantUtility.GiveRadiantXP(caster, 50f);
            caster.GetComp<CompStormlight>().drawStormlight(Props.stormLightCost);
        }
    }
}