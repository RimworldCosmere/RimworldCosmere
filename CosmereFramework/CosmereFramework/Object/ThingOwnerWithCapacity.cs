using UnityEngine;
using Verse;

namespace Cosmere.Framework.Object;

public class ThingOwnerWithCapacity<T> : ThingOwner<T> where T : Verse.Thing {
    private readonly int maxCapacity;

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

    public int remainingCapacity => maxCapacity - TotalStackCount;

    public override int GetCountCanAccept(Verse.Thing? item, bool canMergeWithExistingStacks = true) {
        return Mathf.Min(remainingCapacity, item?.stackCount ?? 0);
    }
}