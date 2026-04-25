namespace Cosmere.Lightweave.Runtime;

public readonly record struct HookKey(int ParentPathHash, int CallSiteId, object? ExplicitKey) {
    public string ToFocusString() {
        return $"lw#{ParentPathHash:X}#{CallSiteId:X}#{ExplicitKey?.GetHashCode():X}";
    }
}