using Cosmere.Core.Settings.Model;
using Verse;

namespace Cosmere.Core.Settings;

public abstract class CosmereModSettings : IExposable {
    public abstract string Name { get; }

    public virtual string SkinId => Name;

    public virtual bool Enabled => true;

    public virtual void ExposeData() { }

    public abstract IReadOnlyList<SettingSection> BuildSections();
}
