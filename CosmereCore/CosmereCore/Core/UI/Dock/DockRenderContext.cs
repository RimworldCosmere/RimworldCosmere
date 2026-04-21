namespace Cosmere.Core.UI.Dock;

public enum DockDensityMode {
    Full,
    Compact,
}

public sealed class DockRenderContext {
    public Dictionary<string, TwinbornPair> TwinbornPairs { get; init; } = new();
    public DockDensityMode Density { get; init; } = DockDensityMode.Full;
}
