using System;
using System.Collections.Generic;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Navigation;

public sealed record MenuItem(
    string Label,
    Action? OnInvoke = null,
    LightweaveNode? Icon = null,
    bool Disabled = false,
    IReadOnlyList<MenuItem>? Children = null);
