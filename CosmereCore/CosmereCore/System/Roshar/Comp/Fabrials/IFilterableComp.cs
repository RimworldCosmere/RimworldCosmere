using Verse;

namespace Cosmere.System.Roshar.Comp.Fabrials;

public interface IFilterableComp {
    List<ThingDef> AllowedSpheres { get; }

    List<ThingDef> FilterList { get; }
}
