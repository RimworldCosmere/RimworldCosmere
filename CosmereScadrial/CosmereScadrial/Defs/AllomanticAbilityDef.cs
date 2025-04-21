using System.Collections.Generic;
using CosmereScadrial.Abilities;
using CosmereScadrial.Abilities.Hediffs;
using RimWorld;
using Verse;

namespace CosmereScadrial.Defs {
    public class AllomanticAbilityDef : AbilityDef {
        public float beuPerTick = 0.1f / GenTicks.TicksPerRealSecond;
        public HediffDef dragHediff;
        public bool followTarget = true;
        public HediffDef hediff;
        public float hediffSeverityFactor = 1f;
        public MetallicArtsMetalDef metal;
        public float minFollowDistance = 5f;
        public float minSeverityForDrag = 1f;
        public bool toggleable;

        public AllomanticAbilityDef() {
            abilityClass = typeof(AllomanticAbility);
            // gizmoClass = typeof(BurnGizmo);
        }

        public override IEnumerable<string> ConfigErrors() {
            foreach (var error in base.ConfigErrors()) {
                yield return error;
            }

            if (hediff == null) {
                yield return "hediff is null";
            }

            if (hediff?.hediffClass != typeof(AllomanticHediff)) {
                yield return "hediffClass is not BurningMetal";
            }

            if (metal == null) {
                yield return "metal is null";
            }
        }
    }
}