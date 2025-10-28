using Verse;

namespace Cosmere.Extension;

public static class ThingOwnerExtension {
    public static int TotalStackCountOfDef(this ThingOwner<Verse.Thing> owner, ThingDef def, ThingDef stuff) {
        return owner.InnerListForReading.Where(thing => thing.def.Equals(def) && thing.Stuff.Equals(stuff))
            .Sum(thing => thing.stackCount);
    }
}