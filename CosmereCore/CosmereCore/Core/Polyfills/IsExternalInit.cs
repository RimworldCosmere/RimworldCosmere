using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

// Polyfill: required by C# record `init` setters when targeting net48.
[SuppressMessage("Major Code Smell", "S2094", Justification = "required by C# record init setters on net48")]
internal static class IsExternalInit { }
