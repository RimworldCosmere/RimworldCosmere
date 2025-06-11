using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs {
    public class DuraluminChargeHediff : AllomanticHediff {
        public override void Tick() {
            base.Tick();

            if (pawn?.Map == null || pawn.Dead) return;

            var duralumin = AllomancyUtility.GetReservePercent(pawn, MetallicArtsMetalDefOf.Duralumin);
            if (duralumin <= 0f) return;

            var burningComp = pawn.TryGetComp<MetalBurning>();
            var reserveComp = pawn.TryGetComp<MetalReserves>();
            if (burningComp == null || reserveComp == null) return;

            if (burningComp.IsBurning()) ProcessBurn();
        }

        public void ProcessBurn() {
            var burningComp = pawn.TryGetComp<MetalBurning>();
            var reserveComp = pawn.TryGetComp<MetalReserves>();
            if (burningComp == null || reserveComp == null) return;

            var toRemove = new List<MetallicArtsMetalDef>();
            var map = burningComp.burnSourceMap.ToList();
            foreach (var ((metal, abilityDef), rate) in map) {
                if (rate <= 0f) continue;

                var ability = pawn.abilities.GetAbility(abilityDef);
                if (ability is not AbstractAbility allomanticAbility) continue;

                allomanticAbility.UpdateStatus(BurningStatus.Duralumin);
                toRemove.Add(metal);
            }

            toRemove.ForEach(reserveComp.RemoveReserve);
            reserveComp.RemoveReserve(MetallicArtsMetalDefOf.Duralumin);

            // Optional: Flash visual
            FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.2f);

            var duraluminAbility =
                pawn.abilities.GetAbility(AbilityDefOf.Cosmere_Ability_Duralumin_Surge) as DuraluminAbility;
            duraluminAbility?.UpdateStatus(BurningStatus.Off);
        }
    }
}