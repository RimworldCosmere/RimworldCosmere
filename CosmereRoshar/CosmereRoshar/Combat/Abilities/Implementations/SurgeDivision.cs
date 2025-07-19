using System.Collections.Generic;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Need;
using RimWorld;
using Verse;

namespace CosmereRoshar.Combat.Abilities.Implementations;

public class SurgeDivisionProperties : CompProperties_AbilityEffect {
    public float stormLightCost;

    public SurgeDivisionProperties() {
        compClass = typeof(SurgeDivision);
    }
}

public class SurgeDivision : CompAbilityEffect {
    public new SurgeDivisionProperties props => (SurgeDivisionProperties)base.props;

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

        if (caster.GetComp<Stormlight>() == null) {
            Log.Error("[Division surge] no stormlight comp!");
            return;
        }

        TryToUseDivision(target.Thing as Pawn, caster);
    }

    private void TryToUseDivision(Pawn? victim, Pawn caster) {
        if (victim == null) return;
        if (!caster.TryGetComp(out Stormlight stormlight)) return;
        if (stormlight.currentStormlight < props.stormLightCost) return;
        List<Verse.Thing> thingsIgnoredByExplosion = [caster];
        if (
            !StormlightUtilities.RollTheDice(1, 25, 2) ||
            !(stormlight.currentStormlight >= props.stormLightCost * 1.5f)
        ) {
            victim.TryAttachFire(Rand.Range(0.95f, 0.99f), caster);
            RadiantUtility.GiveRadiantXp(caster, 50f);
            stormlight.DrawStormlight(props.stormLightCost);
            return;
        }

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
        stormlight.DrawStormlight(props.stormLightCost * 1.5f);
    }
}