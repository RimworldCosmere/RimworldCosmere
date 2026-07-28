using Concord;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Core.Patch.InnerStorage;

[Patch]
public abstract class EnrouteManagerAddEnroutePatch : EnrouteManager {
    protected EnrouteManagerAddEnroutePatch(Map map) : base(map) { }

    [InjectMethod("GetOrAddTracker")]
    protected abstract ThingCountTracker GetOrAddTrackerFor(IHaulEnroute container);

    [Inject(At.Head, nameof(AddEnroute))]
    private Control BeforeAddEnroute(IHaulEnroute container, Pawn pawn, ThingDef stuff, int count) {
        if (container is not Comp.Thing.InnerStorage innerStorage) return Control.Continue;

        ThingCountTracker tracker = GetOrAddTrackerFor(innerStorage);
        tracker.Add(pawn, stuff, count);

        pawn.MapHeld.events.Notify_HaulEnrouteAdded(innerStorage.ParentThing, pawn, stuff, count);
        return Control.Cancel;
    }
}

[Patch]
public abstract class ThingCountTrackerParentThingPatch : ThingCountTracker {
    [Inject(At.Head, nameof(ParentThing))]
    private Control BeforeParentThing(ControlHandle<Verse.Thing> ch) {
        if (parent is not Comp.Thing.InnerStorage innerStorage) return Control.Continue;

        ch.ReturnValue = innerStorage.ParentThing!;
        return Control.Cancel;
    }
}
