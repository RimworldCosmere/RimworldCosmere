using System;
using System.IO;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Options.Tabs;

public static class DeveloperTab {
    public static LightweaveNode Build() {
        return Stack.Create(SpacingScale.Lg, s => {
            s.Add(SettingRow.Section("CL_Options_Section_Developer",
                SettingRow.Create(
                    "CL_Options_TestMapSizes".Translate(),
                    Switch.Create("", Prefs.TestMapSizes, v => Prefs.TestMapSizes = v)
                ),
                SettingRow.Create(
                    "CL_Options_LogVerbose".Translate(),
                    Switch.Create("", Prefs.LogVerbose, v => Prefs.LogVerbose = v)
                ),
                SettingRow.Create(
                    "CL_Options_ResetModsConfigOnCrash".Translate(),
                    Switch.Create("", Prefs.ResetModsConfigOnCrash, v => Prefs.ResetModsConfigOnCrash = v)
                ),
                SettingRow.Create(
                    "CL_Options_DisableQuickStart".Translate(),
                    Switch.Create("", Prefs.DisableQuickStartCryptoSickness, v => Prefs.DisableQuickStartCryptoSickness = v)
                ),
                SettingRow.Create(
                    "CL_Options_StartDevPalette".Translate(),
                    Switch.Create("", Prefs.StartDevPaletteOn, v => Prefs.StartDevPaletteOn = v)
                ),
                SettingRow.Create(
                    "CL_Options_OpenLogOnWarnings".Translate(),
                    Switch.Create("", Prefs.OpenLogOnWarnings, v => Prefs.OpenLogOnWarnings = v)
                ),
                SettingRow.Create(
                    "CL_Options_CloseLogOnEsc".Translate(),
                    Switch.Create("", Prefs.CloseLogWindowOnEscape, v => Prefs.CloseLogWindowOnEscape = v)
                ),
                SettingRow.Create(
                    "CL_Options_DisableDevMode".Translate(),
                    Button.Create(
                        label: "CL_Options_DisableDevMode_Action".Translate(),
                        onClick: () => Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            "ConfirmPermanentlyDisableDevMode".Translate(),
                            DevModePermanentlyDisabledUtility.Disable,
                            destructive: true
                        )),
                        variant: ButtonVariant.Danger,
                        disabled: DevModePermanentlyDisabledUtility.Disabled
                    )
                )
            ));
        });
    }

    

    
}
