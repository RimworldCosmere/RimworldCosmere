using System;
using System.Collections.Generic;
using CosmereScadrial.Abilities;
using Verse;

namespace CosmereScadrial.Comps.Things {
    public class RightClickAbilities : ThingComp {
        public List<RightClickAbility> rightClickAbilities;

        public override void PostPostMake() {
            base.PostPostMake();

            TryAddAbilities();
        }

        public void TryAddAbilities(GeneDef geneDef = null) {
            rightClickAbilities = [];
            if (parent is not Pawn pawn) return;

            if (geneDef != null) {
                rightClickAbilities.AddRange(GetAbilitiesForGene(geneDef));
                return;
            }

            foreach (var gene in pawn.genes?.GenesListForReading ?? []) {
                rightClickAbilities.AddRange(GetAbilitiesForGene(gene.def));
            }
        }

        private IEnumerable<RightClickAbility> GetAbilitiesForGene(GeneDef geneDef) {
            if (geneDef?.GetModExtension<DefModExtensions.RightClickAbilities>() is not { } ext) yield break;

            foreach (var def in ext.abilities) {
                if (Activator.CreateInstance(def.abilityClass, def) is not RightClickAbility ability) continue;
                yield return ability;
            }
        }
    }
}