using System;
using Cosmere.Lightweave.LoadColony;
using Cosmere.Lightweave.Runtime;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(GameDataSaveLoader), nameof(GameDataSaveLoader.SaveGame))]
public static class SaveGameMetadataPatch {
    public static void Postfix(string fileName) {
        try {
            string saveFilePath = GenFilePaths.FilePathForSavedGame(fileName);
            SaveSidecarData data = SaveSidecar.CaptureFromCurrentGame();
            SaveSidecar.Write(saveFilePath, data);
        }
        catch (Exception ex) {
            LightweaveLog.Warning("SaveGameMetadataPatch failed: " + ex);
        }
    }
}
