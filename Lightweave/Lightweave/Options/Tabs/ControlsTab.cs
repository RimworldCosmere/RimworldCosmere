using System;
using RimWorld;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using UnityEngine;
using Verse;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Options.Tabs;

public static class ControlsTab {
    public static LightweaveNode Build() {
        return Stack.Create(SpacingScale.Lg, s => {
            s.Add(SettingRow.Section("CL_Options_Section_Mouse",
                SettingRow.Create(
                    "CL_Options_EdgeScroll".Translate(),
                    Switch.Create("", Prefs.EdgeScreenScroll, v => Prefs.EdgeScreenScroll = v)
                ),
                SettingRow.Create(
                    "CL_Options_ZoomToMouse".Translate(),
                    Switch.Create("", Prefs.ZoomToMouse, v => Prefs.ZoomToMouse = v)
                ),
                SettingRow.Create(
                    "CL_Options_ZoomSwitchLayer".Translate(),
                    Switch.Create("", Prefs.ZoomSwitchWorldLayer, v => Prefs.ZoomSwitchWorldLayer = v)
                ),
                SettingRow.Create(
                    "CL_Options_RememberDrawStyle".Translate(),
                    Switch.Create("", Prefs.RememberDrawStlyes, v => Prefs.RememberDrawStlyes = v)
                ),
                SettingRow.Create(
                    "CL_Options_MapDragSensitivity".Translate(),
                    Slider.Create(
                        value: Prefs.MapDragSensitivity,
                        onChange: v => Prefs.MapDragSensitivity = (float)Math.Round(v, 2),
                        min: 0.8f,
                        max: 2.5f,
                        format: v => Mathf.RoundToInt(v * 100f) + "%"
                    )
                )
            ));
            s.Add(SettingRow.Section("CL_Options_Section_Bindings",
                SettingRow.Create(
                    "CL_Options_KeyBindings".Translate(),
                    Button.Create(
                        label: "CL_Options_KeyBindings_Action".Translate(),
                        onClick: () => Find.WindowStack.Add(new Dialog_KeyBindings()),
                        variant: ButtonVariant.Secondary
                    )
                )
            ));
        });
    }
}
