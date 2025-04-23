using System;
using CosmereScadrial.Abilities;
using CosmereScadrial.Abilities.Hediffs;
using Verse;

namespace CosmereScadrial.Utils {
    public static class HediffUtility {
        public static HediffDef GetHediffDefForPawn(Pawn caster, Pawn target, MultiTypeHediff hediff) {
            if (hediff.getFriendlyHediff() != null && target.Faction == caster.Faction) {
                return hediff.getFriendlyHediff();
            }

            if (hediff.getHostileHediff() != null && target.Faction != caster.Faction) {
                return hediff.getHostileHediff();
            }

            return hediff.getHediff();
        }

        public static AllomanticHediff GetOrAddHediff(Pawn caster, Pawn target, AbstractAllomanticAbility ability, MultiTypeHediff def) {
            var hediffDef = GetHediffDefForPawn(caster, target, def);
            if (TryGetHediff(target, hediffDef, out var hediff)) {
                hediff.AddSource(ability);
                return hediff;
            }

            var newHediff = (AllomanticHediff)Activator.CreateInstance(hediffDef.hediffClass);
            newHediff.def = hediffDef;
            newHediff.pawn = target;
            newHediff.loadID = Find.UniqueIDsManager.GetNextHediffID();
            newHediff.AddSource(ability);
            newHediff.PostMake();

            target.health.AddHediff(newHediff);

            return newHediff;
        }

        public static void RemoveHediff(Pawn caster, Pawn target, AbstractAllomanticAbility ability) {
            var hediffDef = GetHediffDefForPawn(caster, target, ability.def);
            if (!TryGetHediff(target, hediffDef, out var hediff)) {
                return;
            }

            hediff.RemoveSource(ability);
        }

        private static bool TryGetHediff(Pawn target, HediffDef def, out AllomanticHediff hediff) {
            hediff = null;

            target.health.hediffSet.TryGetHediff(def, out var uncastHediff);
            hediff = (AllomanticHediff)uncastHediff;

            return hediff != null;
        }
    }
}