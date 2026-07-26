
using Verse;
namespace Cosmere.Core.Hediff;

public interface IMultiTypeHediff {
    HediffDef? GetHediff();
    HediffDef? GetFriendlyHediff();
    HediffDef? GetHostileHediff();
}