using System.Collections.Generic;
using CosmereScadrial.Abilities;
using CosmereScadrial.Abilities.Hediffs;
using RimWorld;
using Verse;

namespace CosmereScadrial.Defs {
    public class AllomanticAbilityDef : AbilityDef {
        public bool applyDragOnTarget = false;
        public float auraRadius = -1;
        public float beuPerTick = 0.1f / GenTicks.TicksPerRealSecond;
        public HediffDef dragHediff;
        public HediffDef hediff;
        public float hediffSeverityFactor = 1f;
        public MaintenanceDef maintenance;
        public MetallicArtsMetalDef metal;
        public float minSeverityForDrag = 1f;
        public bool toggleable;

        public override IEnumerable<string> ConfigErrors() {
            foreach (var error in base.ConfigErrors()) {
                yield return error;
            }

            if (abilityClass.IsAssignableFrom(typeof(AbstractAllomanticAbility))) {
                yield return $"Invalid ability class {abilityClass}. Must inherit from {typeof(AbstractAllomanticAbility)}.";
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

    public class MaintenanceDef {
        public float followRadius = 10f;
        public JobDef jobDef = JobDefOf.Cosmere_Job_MaintainAllomanticTarget;
    }
}