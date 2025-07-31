using UnityEngine;
using Verse;

namespace Cosmere.Core.Util;

public static class InvestitureDetector {
    public static bool HasInvestiture(Verse.Thing thing) {
        if (IsInsideCoppercloud(thing)) return false;
        if (IsShielded(thing)) return false;

        if (thing is Pawn pawn) {
            return pawn.story?.traits?.HasTrait(TraitDefOf.Cosmere_Invested) ?? false;
        }

        return false;
    }

    public static float GetInvestiture(Verse.Thing thing) {
        if (thing is Pawn pawn) {
            RimWorld.Need? investNeed = pawn.needs?.TryGetNeed(NeedDefOf.Cosmere_Investiture);
            float investiture = investNeed?.CurLevel ?? 0f;
            float scaled = investiture / (investiture + 50000f);
            float normalized = Mathf.Lerp(0.3f, 1f, scaled);

            return normalized;
        }

        return 0f;
    }

    public static bool IsInsideCoppercloud(Verse.Thing? thing) {
        if (!ModsConfig.IsActive("Cosmere.Scadrial")) {
            return false;
        }

        if (thing?.Map == null) return false;

        foreach (Pawn? pawn in thing.Map.mapPawns.AllPawnsSpawned) {
            Verse.Hediff? aura =
                pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("Cosmere_Scadrial_Hediff_CopperAura"));
            if (aura == null) continue;

            float radius = 18f * aura.Severity;
            if ((thing.Position - pawn.Position).LengthHorizontalSquared <= radius * radius) return true;
        }

        return false;
    }

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

                if (!edifice.def.Equals(Resources.ThingDefOf.Aluminum)) return false;
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