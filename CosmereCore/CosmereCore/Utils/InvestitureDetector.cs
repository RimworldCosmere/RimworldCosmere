using RimWorld;
using UnityEngine;
using Verse;
using ThingDefOf = CosmereResources.ThingDefOf;

namespace CosmereCore.Utils;

public static class InvestitureDetector {
    public static bool HasInvestiture(Thing thing) {
        if (IsInsideCoppercloud(thing)) return false;
        if (IsShielded(thing)) return false;

        if (thing is Pawn pawn) {
            return pawn.story?.traits?.HasTrait(TraitDefOf.Cosmere_Invested) ?? false;
        }

        return false;
    }

    public static float GetInvestiture(Thing thing) {
        if (thing is Pawn pawn) {
            Need? investNeed = pawn.needs?.TryGetNeed(NeedDefOf.Cosmere_Investiture);
            float investiture = investNeed?.CurLevel ?? 0f;
            float scaled = investiture / (investiture + 50000f);
            float normalized = Mathf.Lerp(0.3f, 1f, scaled);

            return normalized;
        }

        return 0f;
    }

    public static bool IsInsideCoppercloud(Thing thing) {
        if (!ModsConfig.IsActive("cryptiklemur.cosmere.scadrial")) {
            return false;
        }

        if (thing?.Map == null) return false;

        foreach (Pawn? pawn in thing.Map.mapPawns.AllPawnsSpawned) {
            Hediff? aura = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("Cosmere_Hediff_CopperAura"));
            if (aura == null) continue;

            float radius = 18f * aura.Severity;
            if ((thing.Position - pawn.Position).LengthHorizontalSquared <= radius * radius) return true;
        }

        return false;
    }

    public static bool IsShielded(Thing thing) {
        if (IsInAluminumRoom(thing)) return true;
        if (IsBurningPullingEnhancementMetal(thing as Pawn)) return true;

        return false;
    }

    public static bool IsInAluminumRoom(Thing thing) {
        Room? room = thing.GetRoom();
        if (room == null || room.TouchesMapEdge) return false;

        foreach (Region? region in room.Regions) {
            foreach (IntVec3 cell in region.Cells) {
                Building? edifice = cell.GetEdifice(thing.Map);
                if (edifice == null) continue;

                if (!edifice.def.Equals(ThingDefOf.Aluminum)) return false;
            }
        }

        return true;
    }

    public static bool IsBurningPullingEnhancementMetal(Pawn? pawn) {
        if (!ModsConfig.IsActive("cryptiklemur.cosmere.scadrial") || pawn == null) {
            return false;
        }

        return pawn?.health?.hediffSet?.HasHediff(HediffDef.Named("Cosmere_Hediff_Investiture_Shield")) ?? false;
    }
}