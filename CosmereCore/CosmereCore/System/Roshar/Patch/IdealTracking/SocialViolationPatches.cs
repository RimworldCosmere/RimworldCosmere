using Cosmere.System.Roshar.Surgebinding;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.StartSocialFight))]
public static class SocialFightViolationPatch {
    private static readonly AccessTools.FieldRef<Pawn_InteractionsTracker, Pawn> PawnRef =
        AccessTools.FieldRefAccess<Pawn>(typeof(Pawn_InteractionsTracker), "pawn");

    private static void Postfix(Pawn_InteractionsTracker __instance) {
        Pawn initiator = PawnRef(__instance);
        if (initiator == null) return;

        if (ViolationUtility.IsSurgebinderOfOrder(initiator, "Skybreaker")) {
            ViolationUtility.ApplyViolation(initiator, 0.3f, "starting a social fight");
        }

        if (ViolationUtility.IsSurgebinderOfOrder(initiator, "Bondsmith")) {
            ViolationUtility.ApplyViolation(initiator, 0.1f, "starting a social fight");
        }
    }
}

[HarmonyPatch(typeof(Faction), nameof(Faction.TryAffectGoodwillWith))]
public static class FactionGoodwillViolationPatch {
    private static void Prefix(Faction __instance, Faction other, int goodwillChange, out int __state) {
        __state = __instance.IsPlayer ? __instance.GoodwillWith(other) : -999;
    }

    private static void Postfix(Faction __instance, Faction other, int goodwillChange, bool __result, int __state) {
        if (!__result) return;
        if (__state == -999) return;
        if (!__instance.IsPlayer) return;
        if (goodwillChange >= 0) return;

        int previousGoodwill = __state;
        int currentGoodwill = __instance.GoodwillWith(other);

        bool wentHostile = previousGoodwill >= 0 && currentGoodwill < 0;
        if (!wentHostile) return;

        List<Pawn> colonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (ViolationUtility.IsSurgebinderOfOrder(colonist, "Bondsmith")) {
                ViolationUtility.ApplyViolation(colonist, 0.3f, "faction turned hostile");
            }
        }
    }
}
