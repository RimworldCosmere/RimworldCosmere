namespace Cosmere.Core.UI.Dock;

public enum DockDensityMode {
    Full,
    Compact,
}

public sealed class DockRenderContext {
    public Dictionary<string, TwinbornPair> TwinbornPairs { get; init; } = new Dictionary<string, TwinbornPair>();
    public DockDensityMode Density { get; init; } = DockDensityMode.Full;
}