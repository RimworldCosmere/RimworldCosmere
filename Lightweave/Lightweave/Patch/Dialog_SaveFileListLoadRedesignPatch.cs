using System;
using System.Collections.Generic;
using System.Reflection;
using Cosmere.Lightweave.LoadColony;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Settings;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(Dialog_FileList), nameof(Dialog_FileList.DoWindowContents))]
public static class Dialog_SaveFileListLoadRedesignPatch {
    private static readonly Guid RootId = Guid.NewGuid();
    private static readonly FieldInfo? FilesField = AccessTools.Field(typeof(Dialog_FileList), "files");

    public static bool Prefix(Dialog_FileList __instance, Rect inRect) {
        if (__instance is not Dialog_SaveFileList_Load loadDialog) {
            return true;
        }
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings == null || !settings.RedesignMainMenu) {
            return true;
        }

        try {
            List<SaveFileInfo> files = FilesField?.GetValue(__instance) as List<SaveFileInfo>
                                       ?? new List<SaveFileInfo>();
            LightweaveRoot.Render(inRect, RootId, () => LoadColonyRoot.Build(
                files,
                () => loadDialog.Close()
            ));
        }
        catch (Exception ex) {
            LightweaveLog.Error("LoadColony redesign failed: " + ex);
            return true;
        }
        return false;
    }
}
