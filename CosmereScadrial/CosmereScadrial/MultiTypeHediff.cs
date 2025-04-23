using Verse;

namespace CosmereScadrial {
    public interface MultiTypeHediff {
        HediffDef getHediff();
        HediffDef getFriendlyHediff();
        HediffDef getHostileHediff();
    }
}