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
        // only ever an opinion about spikes - matching anything else would drop the medicine from the bill
        if (!CanEverMatch(t.def)) return false;

        // isCharged, not a null check - a spike can carry invalid charge data, and the comp knows the difference
        HemalurgicSpike? spike = t.TryGetComp<HemalurgicSpike>();

        return spike is not { isCharged: true };
    }

    public override bool CanEverMatch(ThingDef def) {
        return def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike
               || def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;
    }
}
