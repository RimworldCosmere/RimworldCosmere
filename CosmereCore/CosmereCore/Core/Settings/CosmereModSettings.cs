using Cosmere.Core.Settings.Model;
using Verse;

namespace Cosmere.Core.Settings;

public abstract class CosmereModSettings : IExposable {
    public abstract string Name { get; }

    public virtual string SkinId => Name;

    /// <summary>
    ///     The sidebar lists which mod owns the settings; the crest names the investiture
    ///     system, which is a different word for every shard (Roshar vs Surgebinding).
    /// </summary>
    public virtual string DisplayLabel => Name;

    public virtual bool Enabled => true;

    public virtual void ExposeData() { }

    public abstract IReadOnlyList<SettingSection> BuildSections();
}
