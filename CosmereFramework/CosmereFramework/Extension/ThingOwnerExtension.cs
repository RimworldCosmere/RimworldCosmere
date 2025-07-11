using Verse;

namespace CosmereFramework.Extension;

public static class ThingOwnerExtension {
    public static int TotalStackCountOfDef(this ThingOwner<Thing> owner, ThingDef def, ThingDef stuff) {
        int num = 0;
        foreach (Thing thing in owner.InnerListForReading) {
            if (thing.def.Equals(def) && thing.Stuff.Equals(stuff)) {
                num += thing.stackCount;
            }
        }

        return num;
    }
}