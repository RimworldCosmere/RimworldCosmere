using global::System;

namespace Cosmere.Core.Settings.Model;

public sealed record SettingDescriptor {
    public SettingDescriptor(
        string key,
        string labelKey,
        string? descriptionKey,
        SettingControl control,
        Func<bool>? visible = null,
        Func<bool>? enabled = null,
        string? disabledReasonKey = null
    ) {
        Key = key;
        LabelKey = labelKey;
        DescriptionKey = descriptionKey;
        Control = control;
        Visible = visible;
        Enabled = enabled;
        DisabledReasonKey = disabledReasonKey;
    }

    public string Key { get; }

    public string LabelKey { get; }

    public string? DescriptionKey { get; }

    public SettingControl Control { get; }

    public Func<bool>? Visible { get; }

    public Func<bool>? Enabled { get; }

    public string? DisabledReasonKey { get; }

    public bool IsVisible => Visible?.Invoke() ?? true;

    public bool IsEnabled => Enabled?.Invoke() ?? true;
}
