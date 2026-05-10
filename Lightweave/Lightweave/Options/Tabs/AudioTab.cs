using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Options.Tabs;

public static class AudioTab {
    public static LightweaveNode Build() {
        return Stack.Create(SpacingScale.Lg, s => {
            s.Add(SettingRow.Section("CL_Options_Section_Mix",
                SettingRow.Create(
                    "CL_Options_Volume_Master".Translate(),
                    SliderWithReadout.Create(
                        value: Prefs.VolumeMaster,
                        onChange: v => Prefs.VolumeMaster = v,
                        min: 0f,
                        max: 1f,
                        format: FormatPercent
                    )
                ),
                SettingRow.Create(
                    "CL_Options_Volume_Music".Translate(),
                    SliderWithReadout.Create(
                        value: Prefs.VolumeMusic,
                        onChange: v => Prefs.VolumeMusic = v,
                        min: 0f,
                        max: 1f,
                        format: FormatPercent
                    )
                ),
                SettingRow.Create(
                    "CL_Options_Volume_SoundEffects".Translate(),
                    SliderWithReadout.Create(
                        value: Prefs.VolumeGame,
                        onChange: v => Prefs.VolumeGame = v,
                        min: 0f,
                        max: 1f,
                        format: FormatPercent
                    )
                ),
                SettingRow.Create(
                    "CL_Options_Volume_Ambient".Translate(),
                    SliderWithReadout.Create(
                        value: Prefs.VolumeAmbient,
                        onChange: v => Prefs.VolumeAmbient = v,
                        min: 0f,
                        max: 1f,
                        format: FormatPercent
                    )
                ),
                SettingRow.Create(
                    "CL_Options_Volume_UI".Translate(),
                    SliderWithReadout.Create(
                        value: Prefs.VolumeUI,
                        onChange: v => Prefs.VolumeUI = v,
                        min: 0f,
                        max: 1f,
                        format: FormatPercent
                    )
                )
            ));
        });
    }

    private static string FormatPercent(float v) {
        return Mathf.RoundToInt(v * 100f) + "%";
    }
}
