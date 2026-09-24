using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class StormlightBatteryProperties : CompProperties {
    public StormlightBatteryProperties() {
        compClass = typeof(StormlightBattery);
    }
}

// bound by def xml as a comp class, so the empty body is the whole point
public class StormlightBattery : StormlightNode { }
