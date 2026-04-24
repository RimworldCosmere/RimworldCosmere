using System.Reflection;
using Cosmere.System.Roshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using DecoyHediff = Cosmere.System.Roshar.Surgebinding.Hediff.Illumination.LightweavingDecoy;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(ColonistBar), "CheckRecacheEntries")]
public static class BondedSprenColonistBarPatch {
    private static FieldInfo? cachedEntriesField;

    private static void Postfix(ColonistBar __instance) {
        cachedEntriesField ??= AccessTools.Field(typeof(ColonistBar), "cachedEntries");
        if (cachedEntriesField == null) return;

        List<ColonistBar.Entry> entries = (List<ColonistBar.Entry>)cachedEntriesField.GetValue(__instance);
        if (entries == null) return;

        for (int i = entries.Count - 1; i >= 0; i--) {
            Pawn? pawn = entries[i].pawn;
            if (pawn == null) continue;
            if (pawn.TryGetComp<CompSprenBond>() != null || DecoyHediff.IsDecoy(pawn)) {
                entries.RemoveAt(i);
            }
        }
    }
}

[HarmonyPatch(
    typeof(Pawn_PlayerSettings),
    nameof(Pawn_PlayerSettings.UsesConfigurableHostilityResponse),
    MethodType.Getter
)]
public static class BondedSprenHostilityPatch {
    private static FieldInfo? pawnField;

    private static void Postfix(ref bool __result, Pawn_PlayerSettings __instance) {
        pawnField ??= AccessTools.Field(typeof(Pawn_PlayerSettings), "pawn");
        Pawn? pawn = pawnField?.GetValue(__instance) as Pawn;
        if (pawn?.TryGetComp<CompSprenBond>() != null) {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(CaravanFormingUtility), nameof(CaravanFormingUtility.AllSendablePawns))]
public static class BondedSprenCaravanPatch {
    private static void Postfix(List<Pawn> __result) {
        for (int i = __result.Count - 1; i >= 0; i--) {
            if (__result[i].TryGetComp<CompSprenBond>() != null || DecoyHediff.IsDecoy(__result[i])) {
                __result.RemoveAt(i);
            }
        }
    }
}

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetDisabledWorkTypes))]
public static class BondedSprenDisableWorkPatch {
    private static void Postfix(List<WorkTypeDef> __result, Pawn __instance) {
        if (__instance.TryGetComp<CompSprenBond>() == null) return;

        List<WorkTypeDef> allTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
        for (int i = 0; i < allTypes.Count; i++) {
            if (!__result.Contains(allTypes[i])) {
                __result.Add(allTypes[i]);
            }
        }
    }
}

[HarmonyPatch(typeof(PawnTable), "RecachePawns")]
public static class BondedSprenWorkTabPatch {
    private static FieldInfo? cachedPawnsField;

    private static void Postfix(PawnTable __instance) {
        cachedPawnsField ??= AccessTools.Field(typeof(PawnTable), "cachedPawns");
        if (cachedPawnsField == null) return;

        List<Pawn> pawns = (List<Pawn>)cachedPawnsField.GetValue(__instance);
        if (pawns == null) return;

        for (int i = pawns.Count - 1; i >= 0; i--) {
            if (pawns[i].TryGetComp<CompSprenBond>() != null || DecoyHediff.IsDecoy(pawns[i])) {
                pawns.RemoveAt(i);
            }
        }
    }
}