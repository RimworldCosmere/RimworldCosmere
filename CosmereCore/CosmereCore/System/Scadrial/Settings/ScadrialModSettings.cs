using System;
using Cosmere.Core.Settings;
using Cosmere.Core.Settings.Model;
using Verse;

namespace Cosmere.System.Scadrial.Settings;

public enum MistsFrequency {
    Daily,
    Weekly,
    Monthly,
}

public class ScadrialModSettings : CosmereModSettings {
    public bool alwaysShowAllomanticAuras;
    public bool enableMists = true;
    public MistsFrequency mistsFrequency = MistsFrequency.Daily;
    public bool pawnsKeepMetalmindsWhenDowned;
    public bool pawnsKeepVialsWhenDowned;

    public override string Name => "Scadrial";

    public override string SkinId => "Allomancy";

    public override IReadOnlyList<SettingSection> BuildSections() {
        IReadOnlyList<SettingSection> sections = [
            new SettingSection(
                "incidents",
                "CS_Settings_Category_Incidents",
                [
                    new SettingDescriptor(
                        "enable-mists",
                        "CS_Settings_MistsEnabled_Label",
                        "CS_Settings_MistsEnabled_Tooltip",
                        new CheckboxControl(
                            () => enableMists,
                            updated => enableMists = updated,
                            true
                        )
                    ),
                    new SettingDescriptor(
                        "mists-frequency",
                        "CS_Settings_MistsFrequency_Label",
                        "CS_Settings_MistsFrequency_Tooltip",
                        new ChoiceControl(
                            () => mistsFrequency.ToString(),
                            updated => {
                                if (Enum.TryParse(updated, out MistsFrequency parsed)) mistsFrequency = parsed;
                            },
                            nameof(MistsFrequency.Daily),
                            () => [
                                Choice.Keyed(nameof(MistsFrequency.Daily), "CS_Settings_MistsFrequency_Daily"),
                                Choice.Keyed(nameof(MistsFrequency.Weekly), "CS_Settings_MistsFrequency_Weekly"),
                                Choice.Keyed(nameof(MistsFrequency.Monthly), "CS_Settings_MistsFrequency_Monthly"),
                            ],
                            false
                        )
                    ),
                ]
            ),
            new SettingSection(
                "pawns",
                "CS_Settings_Category_Pawns",
                [
                    new SettingDescriptor(
                        "keep-vials-on-down",
                        "CS_Settings_PawnsKeepVialOnDown_Label",
                        "CS_Settings_PawnsKeepVialOnDown_Tooltip",
                        new ChoiceControl(
                            () => pawnsKeepVialsWhenDowned.ToString(),
                            updated => {
                                if (bool.TryParse(updated, out bool parsed)) pawnsKeepVialsWhenDowned = parsed;
                            },
                            true.ToString(),
                            () => [
                                Choice.Keyed(true.ToString(), "CC_Settings_Choice_Yes"),
                                Choice.Keyed(false.ToString(), "CC_Settings_Choice_No"),
                            ],
                            false
                        )
                    ),
                    new SettingDescriptor(
                        "keep-metalminds-on-down",
                        "CS_Settings_PawnsKeepMetalmindsOnDown_Label",
                        "CS_Settings_PawnsKeepMetalmindsOnDown_Tooltip",
                        new ChoiceControl(
                            () => pawnsKeepMetalmindsWhenDowned.ToString(),
                            updated => {
                                if (bool.TryParse(updated, out bool parsed)) pawnsKeepMetalmindsWhenDowned = parsed;
                            },
                            true.ToString(),
                            () => [
                                Choice.Keyed(true.ToString(), "CC_Settings_Choice_Yes"),
                                Choice.Keyed(false.ToString(), "CC_Settings_Choice_No"),
                            ],
                            false
                        )
                    ),
                ]
            ),
            new SettingSection(
                "allomancy",
                "CS_Settings_Category_Allomancy",
                [
                    new SettingDescriptor(
                        "always-show-auras",
                        "CS_Settings_AlwaysShowAuras_Label",
                        "CS_Settings_AlwaysShowAuras_Tooltip",
                        new CheckboxControl(
                            () => alwaysShowAllomanticAuras,
                            updated => alwaysShowAllomanticAuras = updated,
                            false
                        )
                    ),
                ]
            ),
        ];

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate(Name, sections);
        foreach (string error in errors) {
            Cosmere.Core.Logger.Error($"Settings descriptor validation failed: {error}");
        }

        return sections;
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref enableMists, "enableMists", true);
        Scribe_Values.Look(ref mistsFrequency, "mistsFrequency");
        Scribe_Values.Look(ref pawnsKeepMetalmindsWhenDowned, "pawnsKeepMetalmindsWhenDowned", true);
        Scribe_Values.Look(ref pawnsKeepVialsWhenDowned, "pawnsKeepVialsWhenDowned", true);
        Scribe_Values.Look(ref alwaysShowAllomanticAuras, "alwaysShowAllomanticAuras");
    }
}
