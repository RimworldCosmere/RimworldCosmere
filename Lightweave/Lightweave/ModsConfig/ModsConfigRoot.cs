using System;
using System.Collections.Generic;
using System.Linq;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.MainMenu;
using Cosmere.Lightweave.Overlay;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using Verse;

namespace Cosmere.Lightweave.ModsConfig;

public static class ModsConfigRoot {
    public static LightweaveNode Build(RimWorld.Page_ModsConfig page, Action onClose) {
        List<ModMetaData> mods = ModListsInOrder();
        Hooks.Hooks.StateHandle<string?> selected = Hooks.Hooks.UseState<string?>(InitialSelection(mods));
        ModMetaData? activeMod = ResolveActive(mods, selected.Value);

        return Stack.Create(SpacingScale.None, root => {
            root.Add(DialogHeader.Create(
                title: "CL_ModsConfig_Title".Translate(),
                breadcrumb: "CL_Dialog_Crumb_Main".Translate() + " / " + "CL_ModsConfig_Title".Translate(),
                trailingActionLabel: "CL_ModsConfig_Save".Translate(),
                onTrailingAction: onClose,
                onClose: onClose,
                drawDivider: true
            ));
            root.AddFlex(HStack.Create(SpacingScale.None, h => {
                h.Add(ModListPane.Create(
                    mods,
                    selected.Value,
                    name => selected.Set(name),
                    page
                ), new Rem(22f).ToPixels());
                h.AddFlex(ModDetailPane.Create(activeMod, page, onClose));
            }));
        });
    }

    private static List<ModMetaData> ModListsInOrder() {
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
    }

    private static string? InitialSelection(List<ModMetaData> mods) {
        return mods.Count > 0 ? mods[0].PackageId : null;
    }

    private static ModMetaData? ResolveActive(List<ModMetaData> mods, string? packageId) {
        if (string.IsNullOrEmpty(packageId)) {
            return mods.FirstOrDefault();
        }
        return mods.FirstOrDefault(m => string.Equals(m.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
               ?? mods.FirstOrDefault();
    }
}
