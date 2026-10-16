using System.Diagnostics.CodeAnalysis;
using Cosmere.Core.Thing;

namespace Cosmere.System.Roshar.Thing.Pawn.Animal;

// bound by def xml as thingClass, so the empty body is the whole point
[SuppressMessage("Major Code Smell", "S2094", Justification = "bound by def xml as thingClass, so the empty body is the whole point")]
public class Spren : Splinter { }
