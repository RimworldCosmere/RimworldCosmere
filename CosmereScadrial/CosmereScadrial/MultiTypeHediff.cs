using Verse;

namespace CosmereScadrial;

public interface IMultiTypeHediff {
    HediffDef? GetHediff();
    HediffDef? GetFriendlyHediff();
    HediffDef? GetHostileHediff();
}