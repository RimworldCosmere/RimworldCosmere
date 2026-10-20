using System.Diagnostics.CodeAnalysis;
using Cosmere.Core.Settings.Model;

namespace Cosmere.Core.Settings.Search;

public sealed record SettingsSearchDocument {
    [SuppressMessage("Major Code Smell", "S107", Justification = "every parameter sets one property on this value object, so the list is the type's own field set")]
    public SettingsSearchDocument(
        string systemKey,
        string systemName,
        string sectionKey,
        string sectionName,
        string settingKey,
        string label,
        string? description,
        SettingSection? sourceSection = null,
        SettingDescriptor? sourceDescriptor = null
    ) {
        SystemKey = systemKey;
        SystemName = systemName;
        SectionKey = sectionKey;
        SectionName = sectionName;
        SettingKey = settingKey;
        Label = label;
        Description = description;
        SourceSection = sourceSection;
        SourceDescriptor = sourceDescriptor;
    }

    public string SystemKey { get; }

    public string SystemName { get; }

    public string SectionKey { get; }

    public string SectionName { get; }

    public string SettingKey { get; }

    public string Label { get; }

    public string? Description { get; }

    public SettingSection? SourceSection { get; }

    public SettingDescriptor? SourceDescriptor { get; }
}
