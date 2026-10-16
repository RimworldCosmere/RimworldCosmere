using Verse;

namespace Cosmere.Core.Comp.Thing;

public class PreventDropOnDownedProperties : CompProperties {
    public bool preventDrop = true;

    public PreventDropOnDownedProperties() {
        compClass = typeof(PreventDropOnDowned);
    }
}

public class PreventDropOnDowned : ThingComp {
    private PreventDropOnDownedProperties Props => (PreventDropOnDownedProperties)props;

    public bool PreventDrop => Props.preventDrop;
}
