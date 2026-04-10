using System.Collections.Generic;
using System.Reflection;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Tab;
using HarmonyLib;
using RimWorld;
using Verse;
using DecoyHediff = Cosmere.System.Roshar.Surgebinding.Hediff.Illumination.LightweavingDecoy;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(ColonistBar), "CheckRecacheEntries")]
public static class BondedSprenColonistBarPatch {
    private static FieldInfo? cachedEntriesField;

    static void Postfix(ColonistBar __instance) {
        cachedEntriesField ??= AccessTools.Field(typeof(ColonistBar), "cachedEntries");
        if (cachedEntriesField == null) return;

        List<ColonistBar.Entry> entries = (List<ColonistBar.Entry>)cachedEntriesField.GetValue(__instance);
        if (entries == null) return;

        for (int i = entries.Count - 1; i >= 0; i--) {
            Verse.Pawn? pawn = entries[i].pawn;
            if (pawn == null) continue;
            if (pawn.TryGetComp<CompSprenBond>() != null || DecoyHediff.IsDecoy(pawn)) {
                entries.RemoveAt(i);
            }
        }
    }
}

[HarmonyPatch(typeof(Pawn_PlayerSettings), nameof(Pawn_PlayerSettings.UsesConfigurableHostilityResponse), MethodType.Getter)]
public static class BondedSprenHostilityPatch {
    private static global::System.Reflection.FieldInfo? pawnField;

    static void Postfix(ref bool __result, Pawn_PlayerSettings __instance) {
        pawnField ??= AccessTools.Field(typeof(Pawn_PlayerSettings), "pawn");
        Verse.Pawn? pawn = pawnField?.GetValue(__instance) as Verse.Pawn;
        if (pawn?.TryGetComp<CompSprenBond>() != null) {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.GetInspectTabs))]
public static class RadiantSprenTabPatch {
    private static ITab_SprenBond? cachedTab;

    static IEnumerable<InspectTabBase> Postfix(IEnumerable<InspectTabBase>? values, Verse.Thing __instance) {
        bool alreadyHasTab = false;
        if (values != null) {
            foreach (InspectTabBase tab in values) {
                if (tab is ITab_SprenBond) alreadyHasTab = true;
                yield return tab;
            }
        }

        if (alreadyHasTab) yield break;
        if (__instance is not Verse.Pawn pawn) yield break;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder?.bondedSpren == null) yield break;

        cachedTab ??= new ITab_SprenBond();
        yield return cachedTab;
    }
}

[HarmonyPatch(typeof(RimWorld.Planet.CaravanFormingUtility), nameof(RimWorld.Planet.CaravanFormingUtility.AllSendablePawns))]
public static class BondedSprenCaravanPatch {
    static void Postfix(List<Verse.Pawn> __result) {
        for (int i = __result.Count - 1; i >= 0; i--) {
            if (__result[i].TryGetComp<CompSprenBond>() != null || DecoyHediff.IsDecoy(__result[i])) {
                __result.RemoveAt(i);
            }
        }
    }
}

[HarmonyPatch(typeof(Verse.Pawn), nameof(Verse.Pawn.GetDisabledWorkTypes))]
public static class BondedSprenDisableWorkPatch {
    static void Postfix(List<WorkTypeDef> __result, Verse.Pawn __instance) {
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

    static void Postfix(PawnTable __instance) {
        cachedPawnsField ??= AccessTools.Field(typeof(PawnTable), "cachedPawns");
        if (cachedPawnsField == null) return;

        List<Verse.Pawn> pawns = (List<Verse.Pawn>)cachedPawnsField.GetValue(__instance);
        if (pawns == null) return;

        for (int i = pawns.Count - 1; i >= 0; i--) {
            if (pawns[i].TryGetComp<CompSprenBond>() != null || DecoyHediff.IsDecoy(pawns[i])) {
                pawns.RemoveAt(i);
            }
        }
    }
}
