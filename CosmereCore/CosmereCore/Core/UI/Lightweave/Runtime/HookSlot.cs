namespace Cosmere.Core.UI.Lightweave.Runtime;

public sealed class HookSlot
{
    public object? Value;
    public bool TouchedThisFrame;
    public global::System.Action? Cleanup;
}
