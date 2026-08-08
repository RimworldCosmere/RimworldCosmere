using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

/// <summary>Which metal a spike has to be made of for a filter to accept it.</summary>
public class RequiredSpikeMetal : DefModExtension {
    public string metal = string.Empty;
}

/// <summary>
///     Rejects hemalurgic spikes that are not made of one particular metal.
/// </summary>
/// <remarks>
///     A ThingFilter cannot see what a thing is made of. Its stuff category list allows the
///     material itself as an item, which is how listing the zinc category let bare zinc bars
///     stand in for a spike. The one hook that does get handed the Thing is a special filter's
///     worker, so the metal check lives here.
///     <para>
///         Worn as a <c>specialFiltersToDisallow</c> entry: the worker matches the spikes we do
///         NOT want, and the filter drops whatever it matches.
///     </para>
/// </remarks>
public class SpecialThingFilterWorker_WrongSpikeMetal : SpecialThingFilterWorker {
    private string? metal;
    private bool resolved;

    /// <summary>
    ///     The worker is not given its own def, so it finds itself once and remembers.
    /// </summary>
    private string? Metal {
        get {
            if (resolved) return metal;

            resolved = true;
            List<SpecialThingFilterDef> all = DefDatabase<SpecialThingFilterDef>.AllDefsListForReading;
            for (int i = 0; i < all.Count; i++) {
                if (all[i].Worker != this) continue;

                metal = all[i].GetModExtension<RequiredSpikeMetal>()?.metal;
                break;
            }

            return metal;
        }
    }

    public override bool Matches(Verse.Thing t) {
        string? wanted = Metal;
        if (string.IsNullOrEmpty(wanted)) return false;

        // Only ever an opinion about spikes. Everything else the filter allows is none of its
        // business, and matching it would quietly drop medicine from the same bill.
        if (t.def != HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike
            && t.def != HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle) {
            return false;
        }

        return t.Stuff?.defName != wanted;
    }

    public override bool CanEverMatch(ThingDef def) {
        return def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike
               || def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;
    }
}
