using Cosmere.System.Roshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(LifeStageWorker_HumanlikeAdult), nameof(LifeStageWorker_HumanlikeAdult.Notify_LifeStageStarted))]
public static class SprenLifeStagePatch {
    static bool Prefix(Verse.Pawn pawn) {
        if (pawn.def.HasComp(typeof(CompSprenBond))) return false;
        return true;
    }
}
