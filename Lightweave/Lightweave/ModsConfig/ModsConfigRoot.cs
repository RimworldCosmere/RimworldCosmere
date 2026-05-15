using System;
using System.Collections.Generic;
using System.Linq;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Overlay;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Cosmere.Lightweave.ModsConfig;

public enum ModsTab {
    Installed,
    LoadOrder,
}

public static class ModsConfigRoot {
    private static readonly Rem DetailPaneWidth = new Rem(38.75f);

    public static LightweaveNode Build(RimWorld.Page_ModsConfig page, Action onClose) {
        Hooks.Hooks.StateHandle<ModsTab> tab = Hooks.Hooks.UseState(ModsTab.Installed);
        Hooks.Hooks.StateHandle<string> query = Hooks.Hooks.UseState(string.Empty);
        List<ModMetaData> tabMods = ModsForTab(tab.Value);
        List<ModMetaData> visible = FilterByQuery(tabMods, query.Value);
        int activeCount = CountActive();
        int installedCount = CountInstalled();
        int conflictCount = CountConflicts();

        Hooks.Hooks.StateHandle<string?> selected = Hooks.Hooks.UseState<string?>(null);
        ModMetaData? activeMod = ResolveActive(visible, selected.Value);

        string statusLine = "CL_ModsConfig_Stat_ActiveCount".Translate(activeCount.Named("COUNT")).Resolve()
                          + "CL_ModsConfig_Stat_InstalledCount".Translate(installedCount.Named("COUNT")).Resolve();
        string? statusWarn = null;
        if (conflictCount > 0) {
            statusWarn = (conflictCount == 1
                ? "CL_ModsConfig_Stat_ConflictCount".Translate(conflictCount.Named("COUNT"))
                : "CL_ModsConfig_Stat_ConflictCountPlural".Translate(conflictCount.Named("COUNT"))).Resolve();
        }

        List<DialogHeaderTab> tabs = new List<DialogHeaderTab> {
            new DialogHeaderTab(
                (string)"CL_ModsConfig_Tab_Installed".Translate(),
                tab.Value == ModsTab.Installed,
                () => tab.Set(ModsTab.Installed)
            ),
            new DialogHeaderTab(
                (string)"CL_ModsConfig_Tab_LoadOrder".Translate(),
                tab.Value == ModsTab.LoadOrder,
                () => tab.Set(ModsTab.LoadOrder)
            ),
        };

        LightweaveNode card = Box.Create(
            children: c => c.Add(Stack.Create(SpacingScale.None, root => {
                root.Add(DialogHeader.Create(
                    title: "CL_ModsConfig_Title".Translate(),
                    breadcrumb: null,
                    trailingActionLabel: "CL_ModsConfig_Save".Translate(),
                    onTrailingAction: () => SaveAndClose(page),
                    onClose: () => RequestClose(page, onClose),
                    drawDivider: true,
                    tabs: tabs,
                    statusLine: statusLine,
                    statusWarn: statusWarn
                ));
                root.AddFlex(HStack.Create(SpacingScale.None, h => {
                    h.AddFlex(ModListPane.Create(
                        visible,
                        selected.Value,
                        name => selected.Set(name),
                        page,
                        tab.Value,
                        query.Value,
                        q => query.Set(q)
                    ));
                    h.Add(ModDetailPane.Create(activeMod, page, onClose), DetailPaneWidth.ToPixels());
                }));
            }))
        );

        return Dialog.Create(
            content: () => card,
            cardBackground: BackgroundSpec.Blur(new Color(0f, 0f, 0f, 0.95f), 10f)
        );
    }

    private static List<ModMetaData> ModsForTab(ModsTab tab) {
        switch (tab) {
            case ModsTab.LoadOrder:
                return Verse.ModsConfig.ActiveModsInLoadOrder.ToList();
            case ModsTab.Installed:
                List<ModMetaData> result = new List<ModMetaData>();
                foreach (ModMetaData m in Verse.ModsConfig.ActiveModsInLoadOrder) {
                    result.Add(m);
                }
                foreach (ModMetaData m in ModLister.AllInstalledMods) {
                    if (!m.Active) {
                        result.Add(m);
                    }
                }
                return result;
            default:
                return new List<ModMetaData>();
        }
    }


    private static List<ModMetaData> FilterByQuery(List<ModMetaData> mods, string query) {
        if (string.IsNullOrWhiteSpace(query)) {
            return mods;
        }
        List<ModMetaData> result = new List<ModMetaData>();
        foreach (ModMetaData m in mods) {
            string name = m.Name ?? string.Empty;
            string author = m.AuthorsString ?? string.Empty;
            if (name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                || author.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) {
                result.Add(m);
            }
        }
        return result;
    }

    private static int CountActive() {
        int count = 0;
        foreach (ModMetaData _ in Verse.ModsConfig.ActiveModsInLoadOrder) {
            count++;
        }
        return count;
    }

    private static int CountInstalled() {
        int count = 0;
        foreach (ModMetaData _ in ModLister.AllInstalledMods) {
            count++;
        }
        return count;
    }

    private static int CountConflicts() {
        int total = 0;
        foreach (ModMetaData mod in Verse.ModsConfig.ActiveModsInLoadOrder) {
            total += ModConflicts.CountFor(mod);
        }
        return total;
    }

    private static ModMetaData? ResolveActive(List<ModMetaData> mods, string? packageId) {
        if (string.IsNullOrEmpty(packageId)) {
            return mods.FirstOrDefault();
        }
        return mods.FirstOrDefault(m => string.Equals(m.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
               ?? mods.FirstOrDefault();
    }

    private static void SaveAndClose(RimWorld.Page_ModsConfig page) {
        ModsConfigState.SetSaveChanges(page, true);
        page.Close();
    }

    private static void RequestClose(RimWorld.Page_ModsConfig page, Action onClose) {
        if (!ModsConfigState.HasUnsavedChanges(page)) {
            ModsConfigState.SetDiscardChanges(page, true);
            page.Close();
            return;
        }
        Find.WindowStack.Add(new Dialog_ModsConfigConfirmClose(
            onSave: () => SaveAndClose(page),
            onDiscard: () => {
                ModsConfigState.SetDiscardChanges(page, true);
                page.Close();
            }
        ));
    }

    

    

    

    
}
