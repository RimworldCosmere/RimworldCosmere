using Verse;

namespace Cosmere.Core;

public interface IMultiTypeHediff {
    HediffDef? GetHediff();
    HediffDef? GetFriendlyHediff();
    HediffDef? GetHostileHediff();
}