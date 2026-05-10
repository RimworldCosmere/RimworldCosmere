using System;
using System.Collections.Generic;
using System.Globalization;
using RimWorld;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Options.Tabs;

public static class GraphicsTab {
    public static LightweaveNode Build() {
        Resolution[] availableResolutions = ResolutionsAtOrAbove(1024, 768);

        return Stack.Create(SpacingScale.Lg, s => {
            s.Add(SettingRow.Section("CL_Options_Section_Display",
                SettingRow.Create(
                    "CL_Options_Resolution".Translate(),
                    Dropdown.Create(
                        value: ClosestResolution(availableResolutions),
                        options: availableResolutions,
                        labelFn: r => r.width + " x " + r.height,
                        onChange: r => {
                            if (!ResolutionUtility.UIScaleSafeWithResolution(Prefs.UIScale, r.width, r.height)) {
                                Messages.Message("MessageScreenResTooSmallForUIScale".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                                return;
                            }
                            ResolutionUtility.SafeSetResolution(r);
                        }
                    )
                ),
                SettingRow.Create(
                    "CL_Options_Fullscreen".Translate(),
                    Switch.Create("", Screen.fullScreen, v => ResolutionUtility.SafeSetFullscreen(v))
                )
            ));
            s.Add(SettingRow.Section("CL_Options_Section_Visual",
                SettingRow.Create(
                    "CL_Options_TextureCompression".Translate(),
                    Switch.Create("", Prefs.TextureCompression, v => {
                        if (v == Prefs.TextureCompression) {
                            return;
                        }
                        Find.WindowStack.Add(new Dialog_MessageBox(
                            "ChangedTextureCompressionRestart".Translate(),
                            "Yes".Translate(),
                            () => {
                                Prefs.TextureCompression = v;
                                Prefs.Save();
                                GenCommandLine.Restart();
                            },
                            "No".Translate()
                        ));
                        Prefs.TextureCompression = v;
                    })
                ),
                SettingRow.Create(
                    "CL_Options_PlantWindSway".Translate(),
                    Switch.Create("", Prefs.PlantWindSway, v => Prefs.PlantWindSway = v)
                ),
                SettingRow.Create(
                    "CL_Options_ScreenShake".Translate(),
                    Slider.Create(
                        value: Prefs.ScreenShakeIntensity,
                        onChange: v => Prefs.ScreenShakeIntensity = (float)Math.Round(v, 1),
                        min: 0f,
                        max: 2f,
                        format: v => Mathf.RoundToInt(v * 100f) + "%"
                    )
                ),
                SettingRow.Create(
                    "CL_Options_SmoothCamera".Translate(),
                    Switch.Create("", Prefs.SmoothCameraJumps, v => Prefs.SmoothCameraJumps = v)
                ),
                SettingRow.Create(
                    "CL_Options_GravshipCutscenes".Translate(),
                    Switch.Create("", Prefs.GravshipCutscenes, v => Prefs.GravshipCutscenes = v)
                )
            ));
        });
    }

    private static Resolution[] ResolutionsAtOrAbove(int minW, int minH) {
        List<Resolution> all = UnityGUIBugsFixer.ScreenResolutionsWithoutDuplicates;
        List<Resolution> kept = new List<Resolution>();
        for (int i = 0; i < all.Count; i++) {
            if (all[i].width >= minW && all[i].height >= minH) {
                kept.Add(all[i]);
            }
        }
        kept.Sort((a, b) => {
            int c = a.width.CompareTo(b.width);
            return c != 0 ? c : a.height.CompareTo(b.height);
        });
        return kept.ToArray();
    }

    private static Resolution ClosestResolution(Resolution[] options) {
        for (int i = 0; i < options.Length; i++) {
            if (options[i].width == Screen.width && options[i].height == Screen.height) {
                return options[i];
            }
        }
        return options.Length > 0 ? options[options.Length - 1] : new Resolution { width = Screen.width, height = Screen.height };
    }

    
}
