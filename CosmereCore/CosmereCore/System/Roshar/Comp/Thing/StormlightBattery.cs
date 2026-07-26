using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class StormlightBatteryProperties : CompProperties {
    public StormlightBatteryProperties() {
        compClass = typeof(StormlightBattery);
    }
}

public class StormlightBattery : StormlightNode { }