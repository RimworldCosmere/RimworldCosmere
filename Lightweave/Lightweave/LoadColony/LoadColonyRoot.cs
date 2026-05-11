using System;
using System.Collections.Generic;
using System.IO;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.LoadColony;

public static class LoadColonyRoot {
    public static LightweaveNode Build(List<SaveFileInfo> files, Action onClose) {
        Hooks.Hooks.StateHandle<string> filter = Hooks.Hooks.UseState<string>("all");
        Hooks.Hooks.StateHandle<string?> selected = Hooks.Hooks.UseState<string?>(InitialSelection(files));

        List<SaveFileInfo> filteredFiles = ApplyFilter(files, filter.Value);
        SaveFileInfo? activeFile = ResolveActive(filteredFiles, selected.Value) ?? ResolveActive(files, selected.Value);
        SaveStatusInspector.SaveStatus? activeStatus = activeFile != null
            ? SaveStatusInspector.Inspect(activeFile)
            : null;

        List<DialogHeaderTab> tabs = new List<DialogHeaderTab> {
            new DialogHeaderTab("CL_LoadColony_Filter_All".Translate(), filter.Value == "all", () => filter.Set("all")),
            new DialogHeaderTab("CL_LoadColony_Filter_Manual".Translate(), filter.Value == "manual", () => filter.Set("manual")),
            new DialogHeaderTab("CL_LoadColony_Filter_Auto".Translate(), filter.Value == "auto", () => filter.Set("auto")),
        };

        return Box.Create(
            children: c => c.Add(Stack.Create(SpacingScale.None, root => {
                root.Add(DialogHeader.Create(
                    title: "CL_LoadColony_Title".Translate(),
                    breadcrumb: "CL_Dialog_Crumb_Main".Translate() + " / " + "CL_LoadColony_Title".Translate(),
                    onClose: onClose,
                    drawDivider: true,
                    tabs: tabs
                ));
                root.AddFlex(HStack.Create(SpacingScale.None, h => {
                    h.Add(SaveListPane.Create(
                        filteredFiles,
                        selected.Value,
                        name => selected.Set(name)
                    ), new Rem(18f).ToPixels());
                    h.AddFlex(SaveDetailPane.Create(
                        activeFile,
                        activeStatus,
                        onClose,
                        () => SaveStatusInspector.Invalidate(activeFile?.FileInfo.FullName ?? string.Empty)
                    ));
                }));
            })),
            style: new Style {
                Background = BackgroundSpec.Of(ThemeSlot.SurfacePrimary),
                Border = BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
            }
        );
    }

    private static List<SaveFileInfo> ApplyFilter(List<SaveFileInfo> files, string filter) {
        if (files == null) return new List<SaveFileInfo>();
        if (filter == "all") return files;
        bool wantAuto = filter == "auto";
        List<SaveFileInfo> result = new List<SaveFileInfo>();
        for (int i = 0; i < files.Count; i++) {
            string name = Path.GetFileNameWithoutExtension(files[i].FileName);
            bool isAuto = name.StartsWith("Autosave", StringComparison.OrdinalIgnoreCase)
                          || name.StartsWith("autosave", StringComparison.OrdinalIgnoreCase);
            if (isAuto == wantAuto) result.Add(files[i]);
        }
        return result;
    }

    private static string? InitialSelection(List<SaveFileInfo> files) {
        if (files == null || files.Count == 0) {
            return null;
        }
        SaveFileInfo? newest = null;
        foreach (SaveFileInfo file in files) {
            if (newest == null || file.LastWriteTime > newest.LastWriteTime) {
                newest = file;
            }
        }
        return newest != null ? Path.GetFileNameWithoutExtension(newest.FileName) : null;
    }

    private static SaveFileInfo? ResolveActive(List<SaveFileInfo> files, string? selected) {
        if (files == null || string.IsNullOrEmpty(selected)) {
            return null;
        }
        foreach (SaveFileInfo file in files) {
            if (string.Equals(Path.GetFileNameWithoutExtension(file.FileName), selected, StringComparison.OrdinalIgnoreCase)) {
                return file;
            }
        }
        return null;
    }
}
