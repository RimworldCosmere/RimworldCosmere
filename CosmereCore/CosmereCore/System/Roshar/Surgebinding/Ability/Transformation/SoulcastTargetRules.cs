using Verse;
using Cosmere.System.Roshar.Comp.Thing;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transformation;

public static class SoulcastTargetRules {
    public static bool IsValidTargetFor(Verse.Thing thing, SoulcastMode mode) {
        if (thing.Destroyed) return false;
        if (IsBondedSpren(thing)) return false;

        return mode switch {
            SoulcastMode.ConvertDrop => thing.def.category == ThingCategory.Item ||
                                        thing.def.plant != null ||
                                        thing.def.mineable,
            SoulcastMode.ChangeStuff => (thing.def.MadeFromStuff && thing.Stuff != null) || thing.def.mineable,
            SoulcastMode.Sculpture => thing is Pawn or Corpse,
            SoulcastMode.Destroy => thing is not Mote,
            _ => false,
        };
    }

    private static bool IsBondedSpren(Verse.Thing thing) {
        return thing is Pawn p && p.TryGetComp<SprenBond>() != null;
    }
}
