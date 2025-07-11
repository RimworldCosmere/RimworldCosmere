using Verse;

namespace CosmereCore.Comp.Thing;

public class PreventDropOnDownedProperties : CompProperties {
    public bool preventDrop = true;

    public PreventDropOnDownedProperties() {
        compClass = typeof(PreventDropOnDowned);
    }
}

public class PreventDropOnDowned : ThingComp {
    private new PreventDropOnDownedProperties props => (PreventDropOnDownedProperties)base.props;
    public bool preventDrop => props.preventDrop;
}