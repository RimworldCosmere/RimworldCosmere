using System;
using HarmonyLib;
using Verse;

namespace Cosmere.Core.Patch.World;

/// Temporary. Vanilla reports a failed generation without saying what it was
/// trying to make, which leaves the stack pointing at machinery rather than at a
/// pawn. This names the request so the culprit can be identified.
[HarmonyPatch]
public static class PawnGenDiagnosticPatch {
    [HarmonyFinalizer]
    [HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
    public static void Finalizer(Exception? __exception, ref PawnGenerationRequest request) {
        if (__exception == null) return;

        Logger.Error(
            $"PawnGen failed: kind={request.KindDef?.defName ?? "null"} "
            + $"race={request.KindDef?.race?.defName ?? "null"} "
            + $"humanlike={request.KindDef?.RaceProps?.Humanlike.ToString() ?? "null"} "
            + $"faction={request.Faction?.def?.defName ?? "null"} "
            + $"forcedXeno={request.ForcedXenotype?.defName ?? "null"} "
            + $":: {__exception}"
        );
    }
}
