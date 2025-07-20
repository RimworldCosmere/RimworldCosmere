using Verse;

namespace Cosmere.Scadrial;

public interface IMultiTypeHediff {
    HediffDef? GetHediff();
    HediffDef? GetFriendlyHediff();
    HediffDef? GetHostileHediff();
}