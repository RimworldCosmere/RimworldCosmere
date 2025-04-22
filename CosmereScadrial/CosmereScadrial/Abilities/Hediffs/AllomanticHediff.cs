using System;
using System.Text;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Hediffs {
    public class AllomanticHediff : HediffWithComps {
        public AbstractAllomanticAbility sourceAbility;

        private float allomanticPower => sourceAbility?.pawn.GetStatValue(StatDefOf.Cosmere_Allomantic_Power) ?? .25f;

        public override float Severity => sourceAbility?.status switch {
            BurningStatus.Off => sourceAbility.def.hediffSeverityFactor * allomanticPower * 4 * 0f,
            BurningStatus.Passive => sourceAbility.def.hediffSeverityFactor * allomanticPower * 4 * 0.5f,
            BurningStatus.Burning => sourceAbility.def.hediffSeverityFactor * allomanticPower * 4 * 1f,
            BurningStatus.Flaring => sourceAbility.def.hediffSeverityFactor * allomanticPower * 4 * 2f,
            _ => throw new ArgumentOutOfRangeException(),
        };

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