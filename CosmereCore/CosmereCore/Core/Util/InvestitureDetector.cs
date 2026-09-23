using Verse;

namespace Cosmere.Core.Util;

public static class InvestitureDetector {
    public static bool IsShielded(Verse.Thing thing) {
        if (IsInAluminumRoom(thing)) return true;
        if (IsBurningPullingEnhancementMetal(thing as Pawn)) return true;

        return false;
    }

    public static bool IsInAluminumRoom(Verse.Thing thing) {
        Room? room = thing.GetRoom();
        if (room == null || room.TouchesMapEdge) return false;

        foreach (Region? region in room.Regions) {
            foreach (IntVec3 cell in region.Cells) {
                Building? edifice = cell.GetEdifice(thing.Map);
                if (edifice == null) continue;

                if (edifice.Stuff == null || !edifice.Stuff.Equals(ThingDefOf.Aluminum)) return false;
            }
        }

        return true;
    }

    public static bool IsBurningPullingEnhancementMetal(Pawn? pawn) {
        if (!ModsConfig.IsActive("Cosmere.Scadrial") || pawn == null) {
            return false;
        }

        return pawn.health?.hediffSet?.HasHediff(HediffDef.Named("Cosmere_Scadrial_Hediff_InvestitureShield")) ?? false;
    }
}
