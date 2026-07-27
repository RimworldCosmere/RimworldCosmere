using System;
using System.Reflection;
using Cosmere.System.Roshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Spren;

[HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.ShowDraftGizmo), MethodType.Getter)]
public static class BondedSprenDraftGizmoPatch {
    private static FieldInfo? pawnField;

    private static void Postfix(ref bool __result, Pawn_DraftController __instance) {
        if (!__result) return;
        pawnField ??= AccessTools.Field(typeof(Pawn_DraftController), "pawn");
        Pawn? pawn = pawnField?.GetValue(__instance) as Pawn;
        if (pawn?.TryGetComp<SprenBond>() == null) return;
        __result = false;
    }
}

[HarmonyPatch(typeof(Pawn), nameof(Pawn.ThreatDisabled))]
public static class BondedSprenThreatDisabledPatch {
    private static void Postfix(ref bool __result, Pawn __instance) {
        if (__result) return;
        if (__instance.TryGetComp<SprenBond>() == null) return;
        __result = true;
    }
}

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public static class BondedSprenStartJobFilterPatch {
    private static FieldInfo? pawnField;

    private static readonly HashSet<string> BlockedJobDefNames = new HashSet<string>(StringComparer.Ordinal) {
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

    private static bool Prefix(Pawn_JobTracker __instance, Verse.AI.Job newJob) {
        if (newJob?.def == null) return true;
        if (!BlockedJobDefNames.Contains(newJob.def.defName)) return true;

        pawnField ??= AccessTools.Field(typeof(Pawn_JobTracker), "pawn");
        Pawn? pawn = pawnField?.GetValue(__instance) as Pawn;
        if (pawn?.TryGetComp<SprenBond>() == null) return true;

        return false;
    }
}
