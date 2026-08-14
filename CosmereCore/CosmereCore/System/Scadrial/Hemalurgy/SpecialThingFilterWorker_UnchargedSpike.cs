using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

/// <summary>
///     Rejects hemalurgic spikes that have never been driven into anybody.
/// </summary>
/// <remarks>
///     A bare spike is a lump of metal. What makes it worth anything is the charge taken out of
///     somebody it was driven through, and a bill that accepts uncharged ones lets a player make a
///     koloss out of four bars of iron.
///     <para>
///         Worn as a <c>specialFiltersToDisallow</c> entry, the same way the metal filter is: the
///         worker matches the spikes we do NOT want and the filter drops whatever it matches.
///     </para>
/// </remarks>
public class SpecialThingFilterWorker_UnchargedSpike : SpecialThingFilterWorker {
    public override bool Matches(Verse.Thing t) {
        // Only ever an opinion about spikes. Matching anything else would quietly drop the
        // medicine out of the same bill.
        if (!CanEverMatch(t.def)) return false;

        HemalurgicSpike? spike = t.TryGetComp<HemalurgicSpike>();

        return spike?.chargeData == null;
    }

    public override bool CanEverMatch(ThingDef def) {
        return def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike
               || def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;
    }
}
