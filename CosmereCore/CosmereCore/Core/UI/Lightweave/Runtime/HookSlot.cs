using System;

namespace Cosmere.Core.UI.Lightweave.Runtime;

public sealed class HookSlot {
    public Action? Cleanup;
    public bool TouchedThisFrame;
    public object? Value;
}