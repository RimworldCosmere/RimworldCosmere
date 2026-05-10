using System;
using Cosmere.Lightweave.Runtime;

namespace Cosmere.Lightweave.Navigation;

public sealed record MenuItem(
    string Label,
    Action? OnInvoke = null,
    LightweaveNode? Icon = null,
    bool Disabled = false,
    IReadOnlyList<MenuItem>? Children = null,
    bool IsDivider = false,
    bool Danger = false,
    string? Subtitle = null,
    bool Active = false,
    string? Hotkey = null
);