using System.Linq;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs {
    public class DuraluminChargeHediff : AllomanticHediff {
        private MetalBurning burning => pawn.GetComp<MetalBurning>();
        private MetalReserves reserves => pawn.GetComp<MetalReserves>();

        private bool endOnNextTick = false;

        public override void Tick() {
            base.Tick();

            if (endOnNextTick) {
                PostBurn();
                return;
            }

            if (pawn?.Map == null || pawn.Dead) return;

            var duralumin = AllomancyUtility.GetReservePercent(pawn, MetallicArtsMetalDefOf.Duralumin);
            if (duralumin <= 0f) return;

            var burningComp = pawn.TryGetComp<MetalBurning>();
            var reserveComp = pawn.TryGetComp<MetalReserves>();
            if (burningComp == null || reserveComp == null) return;

            if (!burningComp.IsBurning()) return;
            
            Burn();
            endOnNextTick = true;
        }

        public void Burn() {
            foreach (var ((_, abilityDef), rate) in burning.burnSourceMap.ToList()) {
                if (rate <= 0f) continue;

                var ability = pawn.abilities.GetAbility(abilityDef);
                if (ability is not AbstractAbility allomanticAbility) continue;

                allomanticAbility.UpdateStatus(BurningStatus.Duralumin);
            }
        }

        public void PostBurn() {
            burning.GetBurningMetals().ForEach(reserves.RemoveReserve);
            reserves.RemoveReserve(MetallicArtsMetalDefOf.Duralumin);
            FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.2f);

            var ability =
                pawn.abilities.abilities.FirstOrDefault(x =>
                    x.def.defName.Equals(AbilityDefOf.Cosmere_Ability_Duralumin_Surge.defName));
            if (ability is AbstractAbility duraluminAbility) {
                duraluminAbility?.UpdateStatus(BurningStatus.Off);
            }

            pawn.health?.RemoveHediff(this);
        }
    }
}