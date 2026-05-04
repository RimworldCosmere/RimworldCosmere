using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(IdeoFoundation_Deity), "FillDeity")]
public static class SteelMinistryDeityGenderPatch {
    private const string SteelMinistryMemeDefName = "Cosmere_Structure_SteelMinistry";

    public static void Postfix(IdeoFoundation_Deity __instance, IdeoFoundation_Deity.Deity deity) {
        Ideo? ideo = __instance.ideo;
        if (ideo?.StructureMeme == null) return;
        if (ideo.StructureMeme.defName != SteelMinistryMemeDefName) return;

        deity.gender = Gender.Male;
    }
}