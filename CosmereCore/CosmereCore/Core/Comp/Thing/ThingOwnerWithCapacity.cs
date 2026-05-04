using UnityEngine;
using Verse;

namespace Cosmere.Core.Comp.Thing;

public class ThingOwnerWithCapacity<T> : ThingOwner<T> where T : Verse.Thing {
    private int maxCapacity;

    public ThingOwnerWithCapacity() { }

    public ThingOwnerWithCapacity(IThingHolder owner)
        : base(owner) { }

    public ThingOwnerWithCapacity(
        IThingHolder owner,
        LookMode contentsLookMode = LookMode.Deep,
        bool removeContentsIfDestroyed = true
    )
        : base(owner, contentsLookMode, removeContentsIfDestroyed) { }

    public ThingOwnerWithCapacity(
        IThingHolder owner,
        int maxCapacity,
        LookMode contentsLookMode = LookMode.Deep,
        bool removeContentsIfDestroyed = true
    )
        : base(owner, false, contentsLookMode, removeContentsIfDestroyed) {
        this.maxCapacity = maxCapacity;
    }

    public ThingOwnerWithCapacity(
        IThingHolder owner,
        int maxCapacity,
        bool oneStackOnly,
        LookMode contentsLookMode = LookMode.Deep,
        bool removeContentsIfDestroyed = true
    )
        : base(owner, oneStackOnly, contentsLookMode, removeContentsIfDestroyed) {
        this.maxCapacity = maxCapacity;
    }

    public ThingOwnerWithCapacity(
        IThingHolder owner,
        int maxCapacity,
        bool oneStackOnly
    )
        : base(owner, oneStackOnly) {
        this.maxCapacity = maxCapacity;
    }

    public ThingOwnerWithCapacity(
        IThingHolder owner,
        bool oneStackOnly,
        LookMode contentsLookMode = LookMode.Deep,
        bool removeContentsIfDestroyed = true
    )
        : base(owner, oneStackOnly, contentsLookMode, removeContentsIfDestroyed) { }

    public int remainingCapacity => Mathf.Max(maxCapacity - TotalStackCount, 0);

    public override int GetCountCanAccept(Verse.Thing? item, bool canMergeWithExistingStacks = true) {
        if (maxStacks == 1) return base.GetCountCanAccept(item, canMergeWithExistingStacks);

        return Mathf.Min(remainingCapacity, item?.stackCount ?? 0);
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref maxCapacity, "maxCapacity");
    }
}