using Verse;

namespace Cosmere.Framework.Extension;

public static class ThingOwnerExtension {
    public static int TotalStackCountOfDef(this ThingOwner<Verse.Thing> owner, ThingDef def, ThingDef stuff) {
        int num = 0;
        foreach (Verse.Thing thing in owner.InnerListForReading) {
            if (thing.def.Equals(def) && thing.Stuff.Equals(stuff)) {
                num += thing.stackCount;
            }
        }

        return num;
    }
}