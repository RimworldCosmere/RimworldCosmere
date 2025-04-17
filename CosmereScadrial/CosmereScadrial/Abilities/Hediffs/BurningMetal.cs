using System;
using System.Text;
using CosmereScadrial.Utils;
using Verse;

namespace CosmereScadrial.Abilities.Hediffs {
    public class BurningMetal : HediffWithComps {
        public AllomanticAbility sourceAbility;

        /// <summary>
        ///     Severity is based on the BurningStatus of the source ability's ToggleableBurn comp
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override float Severity => sourceAbility.status switch {
            BurningStatus.Off => 0f,
            BurningStatus.Passive => 0.5f,
            BurningStatus.Burning => 1f,
            BurningStatus.Flaring => 2f,
            _ => throw new ArgumentOutOfRangeException(),
        };

        public override void PreRemoved() {
            Log.Message("BurningMetal.PreRemoved");
            base.PreRemoved();
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