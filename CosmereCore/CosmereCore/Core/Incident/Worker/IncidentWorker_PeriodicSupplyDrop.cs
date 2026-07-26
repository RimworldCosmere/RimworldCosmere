using RimWorld;
using Cosmere.Core.DefModExtension;
using Verse;

namespace Cosmere.Core.Incident.Worker;

public class IncidentWorker_PeriodicSupplyDrop : IncidentWorker {
    protected override bool TryExecuteWorker(IncidentParms parms) {
        Map? map = parms.target as Map ?? Find.AnyPlayerHomeMap;
        if (map == null) return false;

        SupplyDropConfig? config = def.GetModExtension<SupplyDropConfig>();
        if (config == null || config.items.Count == 0) {
            Logger.Warning($"PeriodicSupplyDrop: No SupplyDropConfig on IncidentDef '{def.defName}'");
            return false;
        }

        List<Verse.Thing> things = [];
        for (int i = 0; i < config.items.Count; i++) {
            SupplyDropItem entry = config.items[i];
            if (entry.thing == null) continue;

            ThingDef? thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(entry.thing);
            if (thingDef == null) {
                Logger.Warning($"PeriodicSupplyDrop: Thing '{entry.thing}' not found, skipping");
                continue;
            }

            ThingDef? stuffDef = null;
            if (entry.stuff != null) {
                stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(entry.stuff);
            }

            Verse.Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);
            thing.stackCount = entry.count;
            things.Add(thing);
        }

        if (things.Count == 0) return false;

        IntVec3 dropSpot = DropCellFinder.TradeDropSpot(map);
        DropPodUtility.DropThingsNear(dropSpot, map, things, 110, false, true);

        SendStandardLetter(
            def.letterLabel,
            def.letterText,
            def.letterDef ?? LetterDefOf.PositiveEvent,
            parms,
            new TargetInfo(dropSpot, map)
        );

        return true;
    }
}