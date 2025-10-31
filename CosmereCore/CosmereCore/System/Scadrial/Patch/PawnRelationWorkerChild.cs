using Cosmere;
﻿using HarmonyLib;
using RimWorld;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Utility.GeneUtility;

namespace Cosmere.System.Scadrial.Patch;

[HarmonyPatch(typeof(PawnRelationWorker_Child), "CreateRelation")]
public static class PawnRelationWorkerChild {
    public static void Postfix(Pawn generated, Pawn other, PawnGenerationRequest request) {
        GeneUtility.TryAssignScadrialGenes(generated);
        GeneUtility.HandleSkaaPurityGenes(generated, other, request);
    }
}