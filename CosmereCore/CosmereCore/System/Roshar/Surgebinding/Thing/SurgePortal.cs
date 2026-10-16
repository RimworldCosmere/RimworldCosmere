using System.Diagnostics.CodeAnalysis;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Thing;

// bound by def xml as thingClass, so the empty body is the whole point
[SuppressMessage("Major Code Smell", "S2094", Justification = "bound by def xml as thingClass, so the empty body is the whole point")]
public class SurgePortal : ThingWithComps { }
