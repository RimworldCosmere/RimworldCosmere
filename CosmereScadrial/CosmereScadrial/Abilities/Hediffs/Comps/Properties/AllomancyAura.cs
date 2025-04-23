using Verse;

namespace CosmereScadrial.Abilities.Hediffs.Comps.Properties {
    public class AllomancyAura : HediffCompProperties, MultiTypeHediff {
        public HediffDef hediff;
        public HediffDef hediffFriendly;
        public HediffDef hediffHostile;
        public float radius = 18f;
        public string verb;

        public AllomancyAura() {
            compClass = typeof(Comps.AllomancyAura);
        }

        public HediffDef getHediff() {
            return hediff;
        }

        public HediffDef getFriendlyHediff() {
            return hediffFriendly;
        }

        public HediffDef getHostileHediff() {
            return hediffHostile;
        }
    }
}