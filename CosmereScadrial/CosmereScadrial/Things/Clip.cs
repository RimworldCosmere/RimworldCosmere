using CosmereScadrial.Abilities.Allomancy;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Things {
    public class Clip : Bullet {
        private int cachedDamageAmount;

        public override int DamageAmount => cachedDamageAmount;

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget,
            LocalTargetInfo intendedTarget,
            ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null,
            ThingDef targetCoverDef = null) {
            var pawn = launcher as Pawn;
            cachedDamageAmount = Mathf.CeilToInt(def.projectile.GetDamageAmount(1) * GetAbility(pawn).GetStrength());

            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment,
                targetCoverDef);
        }

        private CoinshotAbility GetAbility(Pawn pawn) {
            return (CoinshotAbility)pawn.abilities.GetAbility(AbilityDefOf.Cosmere_Ability_Steel_Coinshot);
        }
    }
}