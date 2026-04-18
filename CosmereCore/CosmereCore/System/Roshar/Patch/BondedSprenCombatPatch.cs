using System.Collections.Generic;
using System.Reflection;
using Cosmere.System.Roshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.ShowDraftGizmo), MethodType.Getter)]
public static class BondedSprenDraftGizmoPatch {
    private static FieldInfo? pawnField;

    static void Postfix(ref bool __result, Pawn_DraftController __instance) {
        if (!__result) return;
        pawnField ??= AccessTools.Field(typeof(Pawn_DraftController), "pawn");
        Verse.Pawn? pawn = pawnField?.GetValue(__instance) as Verse.Pawn;
        if (pawn?.TryGetComp<CompSprenBond>() == null) return;
        __result = false;
    }
}

[HarmonyPatch(typeof(Verse.Pawn), nameof(Verse.Pawn.ThreatDisabled))]
public static class BondedSprenThreatDisabledPatch {
    static void Postfix(ref bool __result, Verse.Pawn __instance) {
        if (__result) return;
        if (__instance.TryGetComp<CompSprenBond>() == null) return;
        __result = true;
    }
}

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public static class BondedSprenStartJobFilterPatch {
    private static FieldInfo? pawnField;

    private static readonly HashSet<string> BlockedJobDefNames = new HashSet<string>(global::System.StringComparer.Ordinal) {
        "Equip",
        "Wear",
        "DropEquipment",
        "RemoveApparel",
        "TakeInventory",
        "HaulToCell",
        "HaulToContainer",
        "AttackMelee",
        "AttackStatic",
        "FlagFromMortar",
        "Hunt",
        "Flee",
        "FleeAndCower",
        "PredatorHunt",
    };

    static bool Prefix(Pawn_JobTracker __instance, Verse.AI.Job newJob) {
        if (newJob?.def == null) return true;
        if (!BlockedJobDefNames.Contains(newJob.def.defName)) return true;

        pawnField ??= AccessTools.Field(typeof(Pawn_JobTracker), "pawn");
        Verse.Pawn? pawn = pawnField?.GetValue(__instance) as Verse.Pawn;
        if (pawn?.TryGetComp<CompSprenBond>() == null) return true;

        return false;
    }
}
