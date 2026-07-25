namespace Cosmere.Core.UI.Dock;

public enum DockDensityMode {
    Full,
    Compact,
}

public sealed class DockRenderContext {
    public DockDensityMode Density { get; init; } = DockDensityMode.Full;
}
