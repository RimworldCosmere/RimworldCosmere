namespace Cosmere.Core.UI.Lightweave.Runtime;

public readonly record struct HookKey(int ParentPathHash, int CallSiteId, object? ExplicitKey)
{
    public string ToFocusString()
        => $"lw#{ParentPathHash:X}#{CallSiteId:X}#{ExplicitKey?.GetHashCode():X}";
}
