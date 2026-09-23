using Cosmere.Core.Settings.Model;
using Verse;

namespace Cosmere.Core.Settings;

public abstract class CosmereModSettings : IExposable {
    public abstract string Name { get; }

    public virtual string SkinId => Name;

    /// <summary>The name the settings window shows, in both the sidebar and the crest.</summary>
    public virtual string DisplayLabel => Name;

    public virtual bool Enabled => true;

    public virtual void ExposeData() { }

    public abstract IReadOnlyList<SettingSection> BuildSections();
}
