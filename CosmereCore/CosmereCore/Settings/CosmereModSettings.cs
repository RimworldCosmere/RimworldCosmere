using Cosmere.Listing;
using Verse;

namespace Cosmere.Settings;

public abstract class CosmereModSettings : IExposable {
    public abstract string Name { get; }
    public virtual bool Enabled => true;

    public virtual void ExposeData() { }

    public abstract void DoTabContents(Form listing);
}