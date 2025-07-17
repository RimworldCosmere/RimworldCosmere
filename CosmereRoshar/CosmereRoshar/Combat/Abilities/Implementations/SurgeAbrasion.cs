using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar {

    public class CompProperties_AbilitySurgeAbrasion : CompProperties_AbilityEffect {

        public CompProperties_AbilitySurgeAbrasion() {
            this.compClass = typeof(CompAbilityEffect_SurgeAbrasion);
        }
    }
    public class CompAbilityEffect_SurgeAbrasion : CompAbilityEffect {
        public new CompProperties_AbilitySurgeAbrasion Props => this.props as CompProperties_AbilitySurgeAbrasion;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            // 1) Validate target
            if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
                Log.Warning("[Abrasion surge] Invalid target.");
                return;
            }


            Pawn caster = this.parent.pawn as Pawn;
            if (caster == null) {
                Log.Error($"[Abrasion surge] caster is null!");
                return;
            }
            if (caster.GetComp<CompStormlight>() == null) {
                Log.Error($"[Abrasion surge] no stormlight comp!");
                return;
            }

            caster.GetComp<CompStormlight>().toggleAbrasion();
        }
    }
}
