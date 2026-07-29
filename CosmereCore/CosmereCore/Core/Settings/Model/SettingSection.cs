using global::System;
using global::System.Collections.Generic;

namespace Cosmere.Core.Settings.Model;

public sealed record SettingSection {
    public SettingSection(
        string key,
        string titleKey,
        IReadOnlyList<SettingDescriptor> settings,
        Func<bool>? visible = null
    ) {
        Key = key;
        TitleKey = titleKey;
        Settings = settings;
        Visible = visible;
    }

    public string Key { get; }

    public string TitleKey { get; }

    public IReadOnlyList<SettingDescriptor> Settings { get; }

    public Func<bool>? Visible { get; }

    public bool IsVisible => Visible?.Invoke() ?? true;
}
