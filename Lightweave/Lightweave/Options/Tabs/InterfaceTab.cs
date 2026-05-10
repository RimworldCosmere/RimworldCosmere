using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cosmere.Lightweave.Fonts;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Settings;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Cosmere.Lightweave.Options.Tabs;

public static class InterfaceTab {
    private const string RandomBackgroundSentinel = "__random__";

    private static readonly int[] FontPresets = { 85, 90, 95, 100, 110, 115, 120, 125 };

    private static readonly TemperatureDisplayMode[] TempModes = {
        TemperatureDisplayMode.Celsius,
        TemperatureDisplayMode.Fahrenheit,
        TemperatureDisplayMode.Kelvin,
    };

    private static readonly ShowWeaponsUnderPortraitMode[] WeaponsUnderPortraitModes = {
        ShowWeaponsUnderPortraitMode.Never,
        ShowWeaponsUnderPortraitMode.WhileDrafted,
        ShowWeaponsUnderPortraitMode.Always,
    };

    private static readonly AnimalNameDisplayMode[] AnimalNameModes = {
        AnimalNameDisplayMode.None,
        AnimalNameDisplayMode.TameNamed,
        AnimalNameDisplayMode.TameAll,
    };

    private static readonly MechNameDisplayMode[] MechNameModes = {
        MechNameDisplayMode.None,
        MechNameDisplayMode.WhileDrafted,
        MechNameDisplayMode.Always,
    };

    private static readonly DotHighlightDisplayMode[] DotHighlightModes = {
        DotHighlightDisplayMode.None,
        DotHighlightDisplayMode.HighlightHostiles,
        DotHighlightDisplayMode.HighlightAll,
    };

    private static readonly HighlightStyleMode[] HighlightStyleModes = {
        HighlightStyleMode.Dots,
        HighlightStyleMode.Silhouettes,
    };

    public static LightweaveNode Build() {
        LightweaveSettings? lwSettings = LightweaveMod.Settings;
        List<ExpansionDef> installedExpansions = ModLister.AllExpansions
            .Where(e => !e.isCore && e.Status != ExpansionStatus.NotInstalled)
            .ToList();
        bool hasBackgroundChoice = installedExpansions.Count > 0;
        bool dotHighlightOn = Prefs.DotHighlightDisplayMode != DotHighlightDisplayMode.None;

        return Stack.Create(SpacingScale.Lg, s => {
            s.Add(SettingRow.Section("CL_Options_Section_Lightweave",
                SettingRow.Create(
                    "CL_Options_FontScale".Translate(),
                    SliderWithReadout.Create(
                        value: lwSettings?.FontScalePercent ?? 100,
                        onChange: v => {
                            if (lwSettings == null) return;
                            int snapped = SnapFont(Mathf.RoundToInt(v));
                            lwSettings.FontScalePercent = snapped;
                            LightweaveMod.Save();
                            GameFontOverride.Apply();
                        },
                        min: 85f,
                        max: 125f,
                        step: 1f,
                        format: v => Mathf.RoundToInt(v) + "%"
                    )
                ),
                SettingRow.Create(
                    "CL_Options_ReduceMotion".Translate(),
                    Switch.Create("", lwSettings?.ReduceMotion ?? false, v => {
                        if (lwSettings == null) return;
                        lwSettings.ReduceMotion = v;
                        LightweaveMod.Save();
                    })
                )
            ));

            s.Add(SettingRow.Section("CL_Options_Section_Display",
                SettingRow.Create(
                    "CL_Options_UIScale".Translate(),
                    SliderWithReadout.Create(
                        value: Prefs.UIScale,
                        onChange: v => {
                            float snapped = SnapUIScale(v);
                            if (snapped != 1f && !ResolutionUtility.UIScaleSafeWithResolution(snapped, Screen.width, Screen.height)) {
                                Messages.Message("MessageScreenResTooSmallForUIScale".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                                return;
                            }
                            ResolutionUtility.SafeSetUIScale(snapped);
                        },
                        min: 1.0f,
                        max: 2.0f,
                        step: 0.25f,
                        format: v => v.ToString("0.00", CultureInfo.InvariantCulture) + "x"
                    )
                ),
                SettingRow.Create(
                    "CL_Options_TemperatureMode".Translate(),
                    Dropdown.Create(
                        value: Prefs.TemperatureMode,
                        options: TempModes,
                        labelFn: m => m.ToStringHuman(),
                        onChange: m => Prefs.TemperatureMode = m
                    )
                ),
                SettingRow.Create(
                    "CL_Options_WeaponsUnderPortrait".Translate(),
                    Dropdown.Create(
                        value: Prefs.ShowWeaponsUnderPortraitMode,
                        options: WeaponsUnderPortraitModes,
                        labelFn: m => m.ToStringHuman(),
                        onChange: m => Prefs.ShowWeaponsUnderPortraitMode = m
                    )
                ),
                SettingRow.Create(
                    "CL_Options_AnimalNames".Translate(),
                    Dropdown.Create(
                        value: Prefs.AnimalNameMode,
                        options: AnimalNameModes,
                        labelFn: m => m.ToStringHuman(),
                        onChange: m => Prefs.AnimalNameMode = m
                    )
                )
            ));

            if (Verse.ModsConfig.BiotechActive) {
                s.Add(SettingRow.Section("CL_Options_Section_Display",
                    SettingRow.Create(
                        "CL_Options_MechNames".Translate(),
                        Dropdown.Create(
                            value: Prefs.MechNameMode,
                            options: MechNameModes,
                            labelFn: m => m.ToStringHuman(),
                            onChange: m => Prefs.MechNameMode = m
                        )
                    )
                ));
            }

            s.Add(SettingRow.Section("CL_Options_Section_Display",
                SettingRow.Create(
                    "CL_Options_DotHighlight".Translate(),
                    Dropdown.Create(
                        value: Prefs.DotHighlightDisplayMode,
                        options: DotHighlightModes,
                        labelFn: m => m.ToStringHuman(),
                        onChange: m => Prefs.DotHighlightDisplayMode = m
                    )
                ),
                SettingRow.Create(
                    "CL_Options_HighlightStyle".Translate(),
                    Dropdown.Create(
                        value: Prefs.HighlightStyleMode,
                        options: HighlightStyleModes,
                        labelFn: m => m.ToStringHuman(),
                        onChange: m => Prefs.HighlightStyleMode = m,
                        disabled: !dotHighlightOn
                    )
                )
            ));

            if (hasBackgroundChoice) {
                s.Add(SettingRow.Section("CL_Options_Section_Background",
                    SettingRow.Create(
                        "CL_Options_BackgroundImage".Translate(),
                        Dropdown.Create(
                            value: Prefs.RandomBackgroundImage
                                ? RandomBackgroundSentinel
                                : Prefs.BackgroundImageExpansion?.defName ?? RandomBackgroundSentinel,
                            options: BackgroundOptionList(installedExpansions),
                            labelFn: id => BackgroundLabelForId(id, installedExpansions),
                            onChange: id => ApplyBackgroundChoice(id, installedExpansions)
                        )
                    )
                ));
            }

            s.Add(SettingRow.Section("CL_Options_Section_Interface",
                SettingRow.Create(
                    "CL_Options_RealtimeClock".Translate(),
                    Switch.Create("", Prefs.ShowRealtimeClock, v => Prefs.ShowRealtimeClock = v)
                ),
                SettingRow.Create(
                    "CL_Options_TwelveHourClock".Translate(),
                    Switch.Create("", Prefs.TwelveHourClockMode, v => Prefs.TwelveHourClockMode = v)
                ),
                SettingRow.Create(
                    "CL_Options_HatsOnlyOnMap".Translate(),
                    Switch.Create("", Prefs.HatsOnlyOnMap, v => {
                        if (v != Prefs.HatsOnlyOnMap) {
                            Prefs.HatsOnlyOnMap = v;
                            PortraitsCache.Clear();
                        }
                    })
                ),
                SettingRow.Create(
                    "CL_Options_DisableTinyText".Translate(),
                    Switch.Create("", Prefs.DisableTinyText, v => {
                        if (SteamDeck.IsSteamDeck) return;
                        if (v == Prefs.DisableTinyText) return;
                        Prefs.DisableTinyText = v;
                        Widgets.ClearLabelCache();
                        GenUI.ClearLabelWidthCache();
                        if (Current.ProgramState == ProgramState.Playing) {
                            Find.ColonistBar.drawer.ClearLabelCache();
                        }
                    })
                ),
                SettingRow.Create(
                    "CL_Options_CustomCursor".Translate(),
                    Switch.Create("", Prefs.CustomCursorEnabled, v => Prefs.CustomCursorEnabled = v)
                ),
                SettingRow.Create(
                    "CL_Options_VisibleMood".Translate(),
                    Switch.Create("", Prefs.VisibleMood, v => Prefs.VisibleMood = v)
                )
            ));
        });
    }

    private static List<string> BackgroundOptionList(List<ExpansionDef> installed) {
        List<string> list = new List<string>();
        foreach (ExpansionDef e in installed) {
            list.Add(e.defName);
        }
        list.Add(RandomBackgroundSentinel);
        return list;
    }

    private static string BackgroundLabelForId(string id, List<ExpansionDef> installed) {
        if (id == RandomBackgroundSentinel) {
            return "CL_Options_BackgroundRandom".Translate();
        }
        for (int i = 0; i < installed.Count; i++) {
            if (installed[i].defName == id) {
                return installed[i].LabelCap;
            }
        }
        return id;
    }

    private static void ApplyBackgroundChoice(string id, List<ExpansionDef> installed) {
        if (id == RandomBackgroundSentinel) {
            Prefs.RandomBackgroundImage = true;
            ExpansionDef? active = ModLister.AllExpansions
                .Where(e => e.Status == ExpansionStatus.Active)
                .RandomElementWithFallback();
            if (active != null && UIMenuBackgroundManager.background is UI_BackgroundMain bgMain) {
                bgMain.overrideBGImage = active.BackgroundImage;
            }
            return;
        }
        for (int i = 0; i < installed.Count; i++) {
            if (installed[i].defName == id) {
                Prefs.BackgroundImageExpansion = installed[i];
                Prefs.RandomBackgroundImage = false;
                return;
            }
        }
    }

    private static int SnapFont(int v) {
        int best = FontPresets[0];
        int bestDist = Mathf.Abs(v - best);
        for (int i = 1; i < FontPresets.Length; i++) {
            int d = Mathf.Abs(v - FontPresets[i]);
            if (d < bestDist) {
                bestDist = d;
                best = FontPresets[i];
            }
        }
        return best;
    }

    private static float SnapUIScale(float v) {
        float[] steps = { 1.0f, 1.25f, 1.5f, 1.75f, 2.0f };
        float best = steps[0];
        float bestDist = Mathf.Abs(v - best);
        for (int i = 1; i < steps.Length; i++) {
            float d = Mathf.Abs(v - steps[i]);
            if (d < bestDist) {
                bestDist = d;
                best = steps[i];
            }
        }
        return best;
    }
}
