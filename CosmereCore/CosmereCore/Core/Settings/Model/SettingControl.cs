using global::System.Diagnostics.CodeAnalysis;

namespace Cosmere.Core.Settings.Model;

[SuppressMessage("Major Code Smell", "S1694", Justification = "this base declares the equality contract the six sealed record controls share, so == and Equals on a SettingControl-typed reference are value comparisons; an interface has no == of its own, which would quietly turn every such comparison into reference equality")]
public abstract record SettingControl {
    public abstract bool IsDefault { get; }

    public abstract void Reset();
}
