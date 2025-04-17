using System;
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
    }
}