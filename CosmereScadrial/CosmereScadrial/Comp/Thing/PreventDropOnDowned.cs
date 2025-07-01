using Verse;

namespace CosmereScadrial.Comp.Thing;

public class PreventDropOnDownedProperties : CompProperties {
    public PreventDropOnDownedProperties() {
        compClass = typeof(PreventDropOnDowned);
    }
}

public class PreventDropOnDowned : ThingComp {
    public static bool PreventDrop => true;
}