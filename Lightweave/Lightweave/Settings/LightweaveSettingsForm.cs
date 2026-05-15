using System;
using Cosmere.Lightweave.Fonts;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Caption = Cosmere.Lightweave.Typography.Typography.Caption;
using Heading = Cosmere.Lightweave.Typography.Typography.Heading;

namespace Cosmere.Lightweave.Settings;

public static class LightweaveSettingsForm {
    private static readonly Guid RootId = Guid.NewGuid();

    public static void Render(Rect inRect) {
        LightweaveRoot.Render(inRect, RootId, Build);
    }

    private static LightweaveNode Build() {
        LightweaveSettings settings = LightweaveMod.Settings;

        return Stack.Create(
            new Rem(1.25f),
            stack => {
                stack.Add(Heading.Create(2, "CL_Settings_Title".Translate()));
                stack.Add(Caption.Create("CL_Settings_Subtitle".Translate()));

                stack.Add(BuildFontSizeSection(settings));
                stack.Add(Divider.Horizontal());

                stack.Add(BuildMainMenuSection(settings));
                stack.Add(Divider.Horizontal());

                stack.Add(BuildAccessibilitySection(settings));
            }
        );
    }

    private static LightweaveNode BuildFontSizeSection(LightweaveSettings settings) {
        return Stack.Create(
            new Rem(0.5f),
            section => {
                section.Add(Heading.Create(3, "CL_Settings_FontSize_Heading".Translate()));
                section.Add(Caption.Create("CL_Settings_FontSize_Help".Translate()));
                section.Add(Slider.Create(
                    value: settings.FontScalePercent,
                    onChange: v => {
                        int snapped = Mathf.RoundToInt(v);
                        if (snapped == settings.FontScalePercent) {
                            return;
                        }
                        settings.FontScalePercent = snapped;
                        LightweaveMod.Save();
                        GameFontOverride.Apply();
                    },
                    min: 75f,
                    max: 150f,
                    step: 5f,
                    marks: new[] { 85f, 100f, 115f, 125f },
                    format: v => Mathf.RoundToInt(v) + "%"
                ));
            }
        );
    }

    private static LightweaveNode BuildMainMenuSection(LightweaveSettings settings) {
        return Stack.Create(
            new Rem(0.5f),
            section => {
                section.Add(Heading.Create(3, "CL_Settings_MainMenu_Heading".Translate()));
                section.Add(Checkbox.Create(
                    label: "CL_Settings_MainMenu_Redesign".Translate(),
                    value: settings.RedesignMainMenu,
                    onChange: v => {
                        settings.RedesignMainMenu = v;
                        LightweaveMod.Save();
                        PromptRestartIfBootDiff(settings);
                    },
                    tooltipKey: "CL_Settings_MainMenu_Redesign_Tip"
                ));
                section.Add(Checkbox.Create(
                    label: "CL_Settings_MainMenu_ParseSaves".Translate(),
                    value: settings.ParseSaveMetadata,
                    onChange: v => {
                        settings.ParseSaveMetadata = v;
                        LightweaveMod.Save();
                    },
                    disabled: !settings.RedesignMainMenu,
                    tooltipKey: "CL_Settings_MainMenu_ParseSaves_Tip"
                ));
            }
        );
    }

    private static void PromptRestartIfBootDiff(LightweaveSettings settings) {
        if (settings.RedesignMainMenu == LightweaveMod.BootRedesignMainMenu) {
            return;
        }
        Find.WindowStack.Add(new Dialog_MessageBox(
            "CL_Settings_Restart_Body".Translate(),
            "CL_Settings_Restart_Confirm".Translate(),
            () => GenCommandLine.Restart(),
            "CL_Settings_Restart_Later".Translate(),
            null,
            "CL_Settings_Restart_Title".Translate()
        ));
    }

    private static LightweaveNode BuildAccessibilitySection(LightweaveSettings settings) {
        return Stack.Create(
            new Rem(0.5f),
            section => {
                section.Add(Heading.Create(3, "CL_Settings_Accessibility_Heading".Translate()));
                section.Add(Checkbox.Create(
                    label: "CL_Settings_ReduceMotion".Translate(),
                    value: settings.ReduceMotion,
                    onChange: v => {
                        settings.ReduceMotion = v;
                        LightweaveMod.Save();
                    },
                    tooltipKey: "CL_Settings_ReduceMotion_Tip"
                ));
            }
        );
    }
}
