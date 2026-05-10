using System;
using System.Collections.Generic;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Navigation;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using RimWorld;
using Verse;

namespace Cosmere.Lightweave.Options.Tabs;

public static class ModSettingsTab {
    public static LightweaveNode Build() {
        List<Mod> modsWithSettings = new List<Mod>();
        foreach (Mod handle in LoadedModManager.ModHandles) {
            if (!string.IsNullOrEmpty(handle.SettingsCategory())) {
                modsWithSettings.Add(handle);
            }
        }

        modsWithSettings.Sort((a, b) =>
            string.Compare(a.SettingsCategory(), b.SettingsCategory(), StringComparison.OrdinalIgnoreCase));

        LightweaveNode[] rows = new LightweaveNode[modsWithSettings.Count];
        for (int i = 0; i < modsWithSettings.Count; i++) {
            Mod captured = modsWithSettings[i];
            rows[i] = MenuRow.Create(
                label: captured.SettingsCategory(),
                onClick: () => Find.WindowStack.Add(new Dialog_ModSettings(captured))
            );
        }

        return Stack.Create(SpacingScale.Lg, s => {
            s.Add(SettingRow.Section("CL_Options_Section_ModSettings", rows));
        });
    }
}
