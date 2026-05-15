using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.MainMenu;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Options.Tabs;

public static class GeneralTab {
    private static readonly float[] AutosaveIntervalPresets = { 0.5f, 1f, 3f, 7f, 14f };

    public static LightweaveNode Build() {
        bool inGame = Current.ProgramState == ProgramState.Playing;
        bool isWindows = Application.platform == RuntimePlatform.WindowsPlayer
                         || Application.platform == RuntimePlatform.WindowsEditor;
        bool showDevToggle = !DevModePermanentlyDisabledUtility.Disabled || Prefs.DevMode;

        return Stack.Create(SpacingScale.Lg, s => {
            s.Add(SettingRow.Section("CL_Options_Section_General",
                SettingRow.Create(
                    "CL_Options_Language".Translate(),
                    LangSelectField.Create(disabled: inGame),
                    caption: inGame
                        ? (string)"ChangeLanguageFromMainMenu".Translate()
                        : (string)"CL_Options_Language_Hint".Translate()
                ),
                SettingRow.Create(
                    "CL_Options_Autosave_Interval".Translate(),
                    Dropdown.Create(
                        value: ClosestAutosaveInterval(Prefs.AutosaveIntervalDays),
                        options: AutosaveIntervalPresets,
                        labelFn: FormatAutosaveInterval,
                        onChange: v => Prefs.AutosaveIntervalDays = v,
                        variant: DropdownVariant.Button,
                        buttonStyle: ButtonVariant.Frosted
                    ),
                    caption: (string)"CL_Options_Autosave_Interval_Hint".Translate()
                ),
                SettingRow.Create(
                    "CL_Options_Autosave_Slots".Translate(),
                    SliderWithReadout.Create(
                        value: Prefs.AutosavesCount,
                        onChange: v => Prefs.AutosavesCount = Mathf.Clamp(Mathf.RoundToInt(v), 1, 25),
                        min: 1f,
                        max: 25f,
                        step: 1f,
                        format: v => Mathf.RoundToInt(v).ToString(CultureInfo.InvariantCulture)
                    ),
                    caption: (string)"CL_Options_Autosave_Slots_Hint".Translate()
                ),
                SettingRow.Create(
                    "CL_Options_RunInBackground".Translate(),
                    Switch.Create("", Prefs.RunInBackground, v => Prefs.RunInBackground = v),
                    caption: (string)"CL_Options_RunInBackground_Hint".Translate()
                )
            ));

            if (showDevToggle) {
                s.Add(SettingRow.Section("CL_Options_Section_General",
                    SettingRow.Create(
                        "CL_Options_DevMode".Translate(),
                        Switch.Create("", Prefs.DevMode, v => Prefs.DevMode = v),
                        caption: (string)"CL_Options_DevMode_Hint".Translate()
                    )
                ));
            }

            s.Add(SettingRow.Section("CL_Options_Section_Files",
                SettingRow.Create(
                    "CL_Options_SaveFolder".Translate(),
                    Button.Create(
                        label: "CL_Options_SaveFolder_Action".Translate(),
                        onClick: () => OpenOrShowFolder(GenFilePaths.SaveDataFolderPath, isWindows),
                        variant: ButtonVariant.Secondary
                    ),
                    caption: GenFilePaths.SaveDataFolderPath
                ),
                SettingRow.Create(
                    "CL_Options_LogFolder".Translate(),
                    Button.Create(
                        label: "CL_Options_LogFolder_Action".Translate(),
                        onClick: () => OpenOrShowFolder(Application.persistentDataPath, isWindows),
                        variant: ButtonVariant.Secondary
                    ),
                    caption: Application.persistentDataPath
                )
            ));

            s.Add(SettingRow.Section("CL_Options_Section_Reset",
                SettingRow.Create(
                    "CL_Options_RestoreDefaults".Translate(),
                    Button.Create(
                        label: "CL_Options_RestoreDefaults_Action".Translate(),
                        onClick: () => Find.WindowStack.Add(new Dialog_MessageBox(
                            "ResetAndRestartConfirmationDialog".Translate(),
                            buttonAAction: RestoreDefaultsAndRestart,
                            buttonAText: "Yes".Translate(),
                            buttonBText: "No".Translate()
                        )),
                        variant: ButtonVariant.Danger
                    ),
                    caption: (string)"CL_Options_RestoreDefaults_Hint".Translate()
                )
            ));
        });
    }

    private static float ClosestAutosaveInterval(float v) {
        float best = AutosaveIntervalPresets[0];
        float bestDist = Mathf.Abs(v - best);
        for (int i = 1; i < AutosaveIntervalPresets.Length; i++) {
            float d = Mathf.Abs(v - AutosaveIntervalPresets[i]);
            if (d < bestDist) {
                bestDist = d;
                best = AutosaveIntervalPresets[i];
            }
        }
        return best;
    }

    private static string FormatAutosaveInterval(float v) {
        string unit = (v == 1f) ? (string)"day".Translate() : (string)"Days".Translate();
        return v.ToString("0.##", CultureInfo.InvariantCulture) + " " + unit;
    }

    private static void OpenOrShowFolder(string path, bool isWindows) {
        if (isWindows) {
            Application.OpenURL(path);
        }
        else {
            Find.WindowStack.Add(new Dialog_MessageBox(Path.GetFullPath(path)));
        }
    }

    private static void RestoreDefaultsAndRestart() {
        FileInfo[] files = new DirectoryInfo(GenFilePaths.ConfigFolderPath).GetFiles("*.xml");
        foreach (FileInfo fileInfo in files) {
            try {
                fileInfo.Delete();
            }
            catch (SystemException) {
            }
        }
        Find.WindowStack.Add(new Dialog_MessageBox("ResetAndRestart".Translate(), null, GenCommandLine.Restart));
    }
}
