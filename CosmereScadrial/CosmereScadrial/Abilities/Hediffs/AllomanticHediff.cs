using System.Text;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Hediffs {
    public class AllomanticHediff : HediffWithComps {
        private float rawPower = .25f;
        public AbstractAllomanticAbility sourceAbility;

        public override void PostAdd(DamageInfo? dinfo) {
            base.PostAdd(dinfo);
            rawPower = sourceAbility.pawn.GetStatValue(StatDefOf.Cosmere_Allomantic_Power);
            Severity = CalculateSeverity();
        }

        public override void Tick() {
            rawPower = sourceAbility.pawn.GetStatValue(StatDefOf.Cosmere_Allomantic_Power);
            Severity = CalculateSeverity();
            base.Tick();
        }

        protected virtual float CalculateSeverity() {
            return (sourceAbility.status ?? BurningStatus.Burning) switch {
                BurningStatus.Off => sourceAbility.def.hediffSeverityFactor * rawPower * 4 * 0f,
                BurningStatus.Passive => sourceAbility.def.hediffSeverityFactor * rawPower * 4 * 0.5f,
                BurningStatus.Burning => sourceAbility.def.hediffSeverityFactor * rawPower * 4 * 1f,
                BurningStatus.Flaring => sourceAbility.def.hediffSeverityFactor * rawPower * 4 * 2f,
                _ => 0f,
            };
        }

        public override string DebugString() {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.DebugString());
            stringBuilder.AppendLine($"Drag Severity={sourceAbility.flareDuration / 3000f:0.000}");
            stringBuilder.AppendLine($"BEUs Required={sourceAbility.def.beuPerTick:0.000}");
            stringBuilder.AppendLine($"Metal Required={AllomancyUtility.GetMetalNeededForBeu(sourceAbility.def.beuPerTick):0.000}");

            return stringBuilder.ToString();
        }
    }
}