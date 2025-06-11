using System.Linq;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Verbs {
    public class Coinshot : Verb_Shoot {
        private CoinshotAbility ability => (CoinshotAbility)verbTracker.directOwner;
        public override ThingDef Projectile => CoinThingDefOf.Cosmere_Clip_Projectile;

        public override void WarmupComplete() {
            ability.UpdateStatus(ability.shouldFlare ? BurningStatus.Flaring : BurningStatus.Burning);

            var duralumin = AllomancyUtility.GetDuraluminBurn(CasterPawn);
            duralumin?.Burn();
            base.WarmupComplete();
            duralumin?.PostBurn();
            ability.UpdateStatus(BurningStatus.Off);
        }

        protected override bool TryCastShot() {
            var pawn = CasterPawn;
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

            return true;
        }
    }
}