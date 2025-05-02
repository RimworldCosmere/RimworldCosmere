using System.Linq;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Verbs {
    public class Coinshot : Verb_Shoot {
        public override ThingDef Projectile {
            get {
                var def = CoinThingDefOf.Cosmere_Clip_Projectile;
                if (verbTracker.directOwner is CoinshotAbility { status: BurningStatus.Flaring }) {
                    def.damageMultipliers.Add(new DamageMultiplier { multiplier = 2, damageDef = DamageDefOf.Bullet });
                }

                return def;
            }
        }

        protected override bool TryCastShot() {
            var pawn = CasterPawn;
            if (verbTracker.directOwner is not CoinshotAbility ability) return false;
            if (pawn?.inventory == null) return false;

            // Find a Clip in inventory
            var clip = pawn.inventory.innerContainer.FirstOrDefault(t => t.def.Equals(CoinThingDefOf.Cosmere_Clip));
            if (clip == null) {
                Messages.Message("No Clips available!", pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            if (!AllomancyUtility.TryBurnMetalForInvestiture(pawn, ability.metal, ability.def.beuPerTick)) return false;
            base.TryCastShot();

            // Consume the clip
            clip.stackCount--;
            if (clip.stackCount <= 0) clip.Destroy();
            ability.UpdateStatus(BurningStatus.Off);

            return true;
        }
    }
}