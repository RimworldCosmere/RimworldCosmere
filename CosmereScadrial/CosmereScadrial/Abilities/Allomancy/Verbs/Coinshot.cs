using System.Linq;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Verbs {
    public class Coinshot : Verb_Shoot {
        protected override bool TryCastShot() {
            var pawn = CasterPawn;
            if (pawn?.inventory == null) return false;

            // Find a Clip in inventory
            var clip = pawn.inventory.innerContainer.FirstOrDefault(t => t.def.Equals(CoinThingDefOf.Cosmere_Clip));
            if (clip == null) {
                Messages.Message("No Clips available!", pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Consume the clip
            clip.Destroy();

            // Fire a projectile (can make a custom one later)
            var projectile = (Projectile)GenSpawn.Spawn(CoinThingDefOf.Cosmere_Clip_Projectile, pawn.Position, pawn.Map);
            projectile.Launch(pawn, currentTarget.Thing, currentTarget.Thing, ProjectileHitFlags.IntendedTarget);
            if (verbTracker.directOwner is CoinshotAbility { status: BurningStatus.Flaring }) {
                projectile.def.damageMultipliers.Add(new DamageMultiplier {
                    multiplier = 2,
                    damageDef = DamageDefOf.Bullet,
                });
            }

            return true;
        }
    }
}