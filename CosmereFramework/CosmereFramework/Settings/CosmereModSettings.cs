using Cosmere.Framework.Listing;
using Verse;

namespace Cosmere.Framework.Settings;

public abstract class CosmereModSettings : IExposable {
    public abstract string Name { get; }
    public virtual bool Enabled => true;

    public virtual void ExposeData() { }

    public abstract void DoTabContents(ListingForm listing);
}