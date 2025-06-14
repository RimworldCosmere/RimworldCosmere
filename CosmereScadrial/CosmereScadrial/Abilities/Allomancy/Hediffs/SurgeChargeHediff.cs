using System;
using System.Linq;
using CosmereScadrial.Comps.Things;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs {
    public class SurgeChargeHediff : AllomanticHediff {
        public Action endCallback;

        public int endInTicks = -1;
        private MetalBurning burning => pawn.GetComp<MetalBurning>();
        private MetalReserves reserves => pawn.GetComp<MetalReserves>();

        public override void Tick() {
            base.Tick();

            switch (endInTicks) {
                case -1:
                    return;
                case 0:
                    PostBurn();
                    break;
            }

            if (endInTicks > 0) endInTicks--;
        }

        public void Burn(Action callback = null, int endInTicks = -1, Action endCallback = null) {
            if (this.endInTicks > -1) return;

            foreach (var ((_, abilityDef), rate) in burning.burnSourceMap.ToList()) {
                if (rate <= 0f) continue;

                var ability = pawn.abilities.GetAbility(abilityDef);
                if (ability is not AbstractAbility allomanticAbility) continue;

                allomanticAbility.UpdateStatus(BurningStatus.Duralumin);
            }

            callback?.Invoke();

            this.endInTicks = endInTicks;
            this.endCallback = endCallback;
        }

        public void PostBurn() {
            burning.GetBurningMetals().ForEach(reserves.RemoveReserve);
            FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.2f);

            foreach (var sourceAbility in sourceAbilities.ToList()) {
                var res = sourceAbility.pawn.GetComp<MetalReserves>();
                if (sourceAbility.def.Equals(AbilityDefOf.Cosmere_Ability_Duralumin_Surge)) {
                    res.RemoveReserve(MetallicArtsMetalDefOf.Duralumin);
                }

                if (sourceAbility.def.Equals(AbilityDefOf.Cosmere_Ability_Nicrosil_Surge)) {
                    res.RemoveReserve(MetallicArtsMetalDefOf.Nicrosil);
                }

                sourceAbility.UpdateStatus(BurningStatus.Off);
            }

            pawn.health?.RemoveHediff(this);
            endCallback?.Invoke();
            endCallback = null;
            endInTicks = -1;
        }
    }
}