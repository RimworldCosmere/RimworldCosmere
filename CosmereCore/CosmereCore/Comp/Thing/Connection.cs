using Verse;

namespace Cosmere.Core.Comp.Thing;

public class ConnectionProperties : CompProperties {
    public bool canBond = true;
    public float maxConnection = 1;

    public ConnectionProperties() {
        compClass = typeof(Connection);
    }
}

public class Connection : ThingComp {
    public bool canBond => props.canBond;
    public float maxConnection => props.maxConnection;
    private new ConnectionProperties props => (ConnectionProperties)base.props;
}