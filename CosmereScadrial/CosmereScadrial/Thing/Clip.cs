using CosmereScadrial.Allomancy.Ability;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Thing;

public class Clip : Bullet {
    private int cachedDamageAmount;

    public override int DamageAmount => cachedDamageAmount;

    public override void Launch(
        Verse.Thing launcher,
        Vector3 origin,
        LocalTargetInfo usedTarget,
        LocalTargetInfo intendedTarget,
        ProjectileHitFlags hitFlags,
        bool preventFriendlyFire = false,
        Verse.Thing? equipment = null,
        ThingDef? targetCoverDef = null
    ) {
        Pawn? pawn = launcher as Pawn;
        if (pawn == null) return;

        cachedDamageAmount =
            Mathf.CeilToInt(def.projectile.GetDamageAmount(1f, null) * GetAbility(pawn).GetStrength());

        base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment,
            targetCoverDef);
    }

    private CoinshotAbility GetAbility(Pawn pawn) {
        return (CoinshotAbility)pawn.abilities.GetAbility(AbilityDefOf.Cosmere_Scadrial_Ability_SteelCoinshot);
    }
}