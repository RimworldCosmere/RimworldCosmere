using System.Diagnostics.CodeAnalysis;
using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class StormlightBatteryProperties : CompProperties {
    public StormlightBatteryProperties() {
        compClass = typeof(StormlightBattery);
    }
}

// bound by def xml as a comp class, so the empty body is the whole point
[SuppressMessage("Major Code Smell", "S2094", Justification = "bound by def xml as a comp class, so the empty body is the whole point")]
public class StormlightBattery : StormlightNode { }
