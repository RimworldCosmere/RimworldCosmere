using Verse;

namespace CosmereCore.Comp.Thing;

public class PreventDropOnDownedProperties : CompProperties {
    public bool preventDrop = true;

    public PreventDropOnDownedProperties() {
        compClass = typeof(PreventDropOnDowned);
    }
}

public class PreventDropOnDowned : ThingComp {
    public PreventDropOnDownedProperties props => (PreventDropOnDownedProperties)base.props;
    public bool PreventDrop => props.preventDrop;
}