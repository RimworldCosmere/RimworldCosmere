namespace Cosmere.Core.Settings.Model;

public abstract record SettingControl {
    public abstract bool IsDefault { get; }

    public abstract void Reset();
}
