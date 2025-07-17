using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar {

    public class CompProperties_AbilityToggleStormlight : CompProperties_AbilityEffect {

        public CompProperties_AbilityToggleStormlight() {
            this.compClass = typeof(CompAbilityEffect_AbilityToggleStormlight);
        }
    }
    public class CompAbilityEffect_AbilityToggleStormlight : CompAbilityEffect {
        public new CompProperties_AbilityToggleStormlight Props => this.props as CompProperties_AbilityToggleStormlight;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            // 1) Validate target
            if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
                Log.Warning("[Toggle Stormlight] Invalid target.");
                return;
            }


            Pawn caster = this.parent.pawn as Pawn;
            if (caster == null) {
                Log.Error($"[Toggle Stormlight] caster is null!");
                return;
            }
            if (caster.GetComp<CompStormlight>() == null) {
                Log.Error($"[Toggle Stormlight] no stormlight comp!");
                return;
            }

            caster.GetComp<CompStormlight>().toggleBreathStormlight();
        }
    }
}
