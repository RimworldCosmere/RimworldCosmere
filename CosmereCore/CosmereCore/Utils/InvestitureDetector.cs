using CosmereMetals;
using UnityEngine;
using Verse;

namespace CosmereCore.Utils {
    public static class InvestitureDetector {
        public static bool HasInvestiture(Thing thing) {
            if (IsInsideCoppercloud(thing)) return false;
            if (IsAluminumShielded(thing)) return false;

            if (thing is Pawn pawn) {
                return pawn.story?.traits?.HasTrait(TraitDefOf.Cosmere_Invested) ?? false;
            }

            return false;
        }

        public static float GetInvestiture(Thing thing) {
            if (thing is Pawn pawn) {
                var investNeed = pawn.needs?.TryGetNeed(NeedDefOf.Cosmere_Investiture);
                var investiture = investNeed?.CurLevel ?? 0f;
                var scaled = investiture / (investiture + 50000f);
                var normalized = Mathf.Lerp(0.3f, 1f, scaled);

                return normalized;
            }

            return 0f;
        }

        public static bool IsInsideCoppercloud(Thing thing) {
            if (!ModsConfig.IsActive("cryptiklemur.cosmere.scadrial")) {
                return false;
            }

            if (thing?.Map == null) return false;

            foreach (var pawn in thing.Map.mapPawns.AllPawnsSpawned) {
                var aura = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("Cosmere_Hediff_CopperAura"));
                if (aura == null) continue;

                var radius = 18f * aura.Severity;
                if ((thing.Position - pawn.Position).LengthHorizontalSquared <= radius * radius) return true;
            }

            return false;
        }

        public static bool IsAluminumShielded(Thing thing) {
            if (IsInAluminumRoom(thing)) return true;
            if (IsBurningAluminum(thing as Pawn)) return true;

            return false;
        }

        public static bool IsInAluminumRoom(Thing thing) {
            var room = thing.GetRoom();
            if (room == null || room.TouchesMapEdge) return false;

            foreach (var region in room.Regions) {
                foreach (var cell in region.Cells) {
                    var edifice = cell.GetEdifice(thing.Map);
                    if (edifice == null) continue;

                    if (!edifice.def.Equals(ThingDefOf.Aluminum)) return false;
                }
            }

            return true;
        }

        public static bool IsBurningAluminum(Pawn pawn) {
            if (!ModsConfig.IsActive("cryptiklemur.cosmere.scadrial")) {
                return false;
            }

            return pawn?.health?.hediffSet?.HasHediff(HediffDef.Named("Cosmere_Hediff_AluminumShield")) ?? false;
        }
    }
}