using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(Page_ChooseIdeoPreset), "DoClassic")]
public static class DoClassicNextGuardPatch {
    private static readonly FieldInfo ClassicIdeoField = AccessTools.Field(
        typeof(Page_ChooseIdeoPreset),
        "classicIdeo"
    );

    public static bool Prefix(Page_ChooseIdeoPreset __instance) {
        Ideo? classicIdeo = ClassicIdeoField.GetValue(__instance) as Ideo;

        List<Faction> factions = Find.FactionManager.AllFactionsListForReading;
        for (int i = 0; i < factions.Count; i++) {
            Faction faction = factions[i];
            if (faction.ideos == null) continue;

            faction.ideos.RemoveAll();
            faction.ideos.SetPrimary(classicIdeo);
        }

        Find.IdeoManager.RemoveUnusedStartingIdeos();
        Find.Scenario.PostIdeoChosen();

        if (__instance.next != null) {
            __instance.next.prev = Find.Storyteller.def.tutorialMode ? __instance.prev : __instance;
            Find.WindowStack.Add(__instance.next);
        }

        Action nextAct = __instance.nextAct;
        if (nextAct != null) {
            nextAct();
        }

        TutorSystem.Notify_Event("PageClosed");
        TutorSystem.Notify_Event("GoToNextPage");
        __instance.Close();

        return false;
    }
}
