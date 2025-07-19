using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
public static class PatchRadiantProgressNeed {
    private static readonly AccessTools.FieldRef<Pawn_NeedsTracker, Pawn> pawnRef =
        AccessTools.FieldRefAccess<Pawn>(typeof(Pawn_NeedsTracker), "pawn");

    public static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result) {
        Pawn? pawn = pawnRef(__instance);
        //if (nd == CosmereRosharDefs.Cosmere_Roshar_Need_RadiantProgress) {
        //    __result = ___pawn.story?.traits?.allTraits.FirstOrDefault(t => CosmereRosharUtilities.RadiantTraits.Contains(t.def)) != null;
        //}

        if (nd != CosmereRosharDefs.Cosmere_Roshar_Need_RadiantProgress) return;
        if (pawn.story?.traits == null) return;

        __result =
            pawn.story.traits.HasTrait(CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner) ||
            pawn.story.traits.HasTrait(CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantTruthwatcher) ||
            pawn.story.traits.HasTrait(CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantEdgedancer) ||
            pawn.story.traits.HasTrait(CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantSkybreaker);
    }
}