using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Scadrial.Thing;

/// <summary>
///     Bound as thingClass in CosmereScadrial/Defs/Allomancy/Things/Buildings.xml, and the type
///     HideStoredVialsPatch checks to hide stored vials. Empty on purpose.
/// </summary>
[SuppressMessage("Major Code Smell", "S2094", Justification = "bound by def xml as thingClass, so the empty body is the whole point")]
public class Building_VialCabinet : Building_Storage { }
