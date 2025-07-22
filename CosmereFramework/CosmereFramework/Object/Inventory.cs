using Verse;

namespace Cosmere.Framework.Object;

public class Inventory(IThingHolder owner) : IThingHolder, IExposable {
    public ThingOwner<Verse.Thing> innerContainer;
    public IThingHolder owner = owner;

    public void ExposeData() {
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
    }

    public void GetChildHolders(List<IThingHolder> outChildren) {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public ThingOwner GetDirectlyHeldThings() {
        return innerContainer;
    }

    public IThingHolder ParentHolder => owner;
}