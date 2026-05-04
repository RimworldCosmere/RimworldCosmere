namespace Cosmere.Core.UI.Dock;

public enum DockDensityMode {
    Full,
    Compact,
}

public sealed class DockRenderContext {
    public Dictionary<string, IDualInvestiturePair> DualInvestiturePairs { get; init; } = new Dictionary<string, IDualInvestiturePair>();
    public DockDensityMode Density { get; init; } = DockDensityMode.Full;
}
