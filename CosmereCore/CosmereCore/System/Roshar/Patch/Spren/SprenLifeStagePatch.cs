using HarmonyLib;
using RimWorld;
using Verse;
using Cosmere.System.Roshar.Comp.Thing;

namespace Cosmere.System.Roshar.Patch.Spren;

[HarmonyPatch(typeof(LifeStageWorker_HumanlikeAdult), nameof(LifeStageWorker_HumanlikeAdult.Notify_LifeStageStarted))]
public static class SprenLifeStagePatch {
    private static bool Prefix(Pawn pawn) {
        if (pawn.def.HasComp(typeof(SprenBond))) return false;
        return true;
    }
}