using System;
using Cosmere.Lightweave.Runtime;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Profile;
using Verse.Sound;

namespace Cosmere.Lightweave.MainMenu;

public static class MainMenuActions {
    public static void NewColony() {
        Find.WindowStack.Add(new Page_SelectScenario());
    }

    public static void OpenLoadDialog() {
        Find.WindowStack.Add(new Dialog_SaveFileList_Load());
    }

    public static void Tutorial() {
        InvokeStaticIfFound("Verse.MainMenuDrawer", "InitLearnToPlay");
    }

    public static void OpenOptions() {
        Find.WindowStack.Add(new Dialog_Options());
    }

    public static void OpenMods() {
        Find.WindowStack.Add(new Page_ModsConfig());
    }

    public static void OpenCredits() {
        Find.WindowStack.Add(new Screen_Credits());
    }

    public static void QuitToOS() {
        Root.Shutdown();
    }

    public static void DevQuickTest() {
        LongEventHandler.QueueLongEvent(() => {
            Root_Play.SetupForQuickTestPlay();
            PageUtility.InitGameStart();
        }, "GeneratingMap", true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
    }

    public static void SaveTranslationReport() {
        LanguageReportGenerator.SaveTranslationReport();
    }

    public static void ContinueLatestSave(string? fileName) {
        if (string.IsNullOrEmpty(fileName)) {
            OpenLoadDialog();
            return;
        }

        SoundDefOf.Click.PlayOneShotOnCamera();
        try {
            GameDataSaveLoader.LoadGame(fileName);
        }
        catch (Exception ex) {
            LightweaveLog.Error("Lightweave continue-load failed for " + fileName + ": " + ex);
            OpenLoadDialog();
        }
    }

    private static void InvokeStaticIfFound(string typeName, string methodName) {
        Type? t = GenTypes.GetTypeInAnyAssembly(typeName);
        if (t == null) {
            return;
        }
        System.Reflection.MethodInfo? m = HarmonyLib.AccessTools.Method(t, methodName);
        m?.Invoke(null, null);
    }
}
